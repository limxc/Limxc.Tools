using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using Limxc.Tools.Extensions;
using Limxc.Tools.Extensions.Communication;

namespace Limxc.Tools.SerialPort
{
    public class SerialPortServiceSimulator : ISerialPortService
    {
        private readonly Subject<bool> _connectionState = new Subject<bool>();
        private readonly CompositeDisposable _initDisposables = new CompositeDisposable();
        private readonly Subject<string> _log = new Subject<string>();

        private readonly int _lostInterval;
        private readonly Subject<byte[]> _received = new Subject<byte[]>();
        private readonly int _sendDelay;

        private int _sendIndex;

        private Dictionary<string, string[]> _dictSendResp;

        public SerialPortServiceSimulator(
            int lostInterval = 0,
            int sendDelay = 100,
            Dictionary<string, string[]> dictSendResp = null
        )
        {
            _dictSendResp = dictSendResp;
            _lostInterval = lostInterval;
            _sendDelay = sendDelay;

            Observable
                .Interval(TimeSpan.FromSeconds(1))
                .Select(_ => IsConnected)
                .Subscribe(s => _connectionState.OnNext(s))
                .DisposeWith(_initDisposables);

            ConnectionState = Observable.Defer(() =>
                _connectionState.StartWith(false).AsObservable().Publish().RefCount()
            );
            Received = Observable.Defer(() => _received.AsObservable().Publish().RefCount());
            Log = Observable.Defer(() => _log.AsObservable().Publish().RefCount());

            var typeName = "";
            try
            {
                var stackTrace = new StackTrace();
                // ReSharper disable once AssignNullToNotNullAttribute
                typeName = stackTrace
                    .GetFrames()
                    .Select(p => p.GetMethod())
                    // ReSharper disable once PossibleNullReferenceException
                    .Where(p => p.DeclaringType.IsInstanceOfType(this))
                    .Select(p => p.DeclaringType?.Name)
                    .LastOrDefault();
            }
            catch
            {
                // ignored
            }

            Log.Subscribe(p =>
                    Debug.WriteLine(
                        $"@{DateTime.Now:HH:mm:ss fff} {(string.IsNullOrEmpty(typeName) ? "" : $"From {typeName} ")}| {p}"
                    )
                )
                .DisposeWith(_initDisposables);
        }

        public bool IsConnected { get; private set; }
        public IObservable<bool> ConnectionState { get; }
        public IObservable<byte[]> Received { get; }
        public IObservable<string> Log { get; }

        private async Task SendResp(string hex)
        {
            hex = hex.Replace(" ", "").ToUpper();

            _log.OnNext($"Send: {hex}");

            if (_dictSendResp?.ContainsKey(hex) ?? false)
            {
                foreach (var resp in _dictSendResp[hex])
                {
                    await Task.Delay(100);
                    _received.OnNext(resp.HexToByte());
                }
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="setting"></param>
        /// <param name="configSerialPort"></param>
        public void Start(SerialPortSetting setting, Action<object> configSerialPort = null)
        {
            _log.OnNext("Start");
            Stop();

            IsConnected = true;
        }

        public void Stop()
        {
            _log.OnNext("Stop");
            IsConnected = false;
        }

        /// <summary>发送成功时, 无返回值</summary>
        public void Send(byte[] bytes)
        {
            _sendIndex++;
            if (_lostInterval > 0 && _sendIndex % _lostInterval == 0) //模拟失败
                _log.OnNext($"Send Err: {bytes.ByteToHex()}");
            else
                _ = SendResp(bytes.ByteToHex());
        }

        /// <summary>发送成功时, 无返回值</summary>
        public async Task SendAsync(byte[] bytes)
        {
            await Task.Delay(_sendDelay);

            _sendIndex++;
            if (_lostInterval > 0 && _sendIndex % _lostInterval == 0) //模拟失败
                _log.OnNext($"Send Err: {bytes.ByteToHex()}");
            else
                await SendResp(bytes.ByteToHex());
        }

        /// <summary>发送成功时, 返回值=<see cref="template" /></summary>
        public async Task<string> SendAsync(
            string hex,
            int timeoutMs,
            string template,
            char sepBegin = '[',
            char sepEnd = ']'
        )
        {
            try
            {
                await Task.Delay(_sendDelay + timeoutMs / 2);

                _sendIndex++;
                if (_lostInterval > 0 && _sendIndex % _lostInterval == 0) //模拟失败
                {
                    _log.OnNext($"Send Err: {hex}");
                    return string.Empty;
                }

                var task = _received
                    .Select(p => p.ByteToHex())
                    .TryGetTemplateMatchResult(template, timeoutMs, sepBegin, sepEnd);

                await SendResp(hex);

                return await task;
            }
            catch (Exception ex)
            {
                _log.OnNext($"Send Error: {ex.Message}");
                return string.Empty;
            }
        }

        /// <summary>发送成功时, 返回值=发送值</summary>
        public async Task<byte[]> SendAsync(byte[] bytes, int waitMs)
        {
            try
            {
                await Task.Delay(_sendDelay + waitMs / 2);
                _sendIndex++;
                if (_lostInterval > 0 && _sendIndex % _lostInterval == 0) //模拟失败
                {
                    _log.OnNext($"Send Err: {bytes.ByteToHex()}");
                    return Array.Empty<byte>();
                }

                var now = DateTimeOffset.Now;
                var task = _received
                    .SkipUntil(now)
                    .TakeUntil(now.AddMilliseconds(waitMs))
                    .Aggregate((x, y) => x.Concat(y).ToArray())
                    .ToTask();

                await SendResp(bytes.ByteToHex());

                return await task;
            }
            catch (Exception ex)
            {
                _log.OnNext($"Send Error: {ex.Message}");
                return Array.Empty<byte>();
            }
        }

        public virtual void Dispose()
        {
            _initDisposables?.Dispose();
            _connectionState?.Dispose();
            _received?.Dispose();
            _log?.Dispose();

            Stop();
        }

        public string[] GetPortNames()
        {
            return new[] { "Simulated COM" };
        }
    }
}
