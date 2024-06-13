using System;
using System.IO;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using Limxc.Tools.Extensions;
using Limxc.Tools.Extensions.Communication;
using SP = System.IO.Ports.SerialPort;

namespace Limxc.Tools.SerialPort
{
    public class SerialPortService : ISerialPortService
    {
        private readonly Subject<bool> _connectionState = new Subject<bool>();
        private readonly CompositeDisposable _initDisposables = new CompositeDisposable();
        private readonly Subject<string> _log = new Subject<string>();
        private readonly Subject<byte[]> _received = new Subject<byte[]>();

        private CompositeDisposable _controlDisposables = new CompositeDisposable();
        private SerialPortSetting _setting;

        private SP _sp;

        public SerialPortService()
        {
            Observable
                .Interval(TimeSpan.FromSeconds(1))
                .Select(_ => IsConnected)
                .SubscribeOn(TaskPoolScheduler.Default)
                .Subscribe(s => _connectionState.OnNext(s))
                .DisposeWith(_initDisposables);

            ConnectionState = Observable.Defer(
                () => _connectionState.StartWith(false).AsObservable().Publish().RefCount()
            );
            Received = Observable.Defer(() => _received.AsObservable().Publish().RefCount());
            Log = Observable.Defer(() => _log.AsObservable().Publish().RefCount());
        }

        public bool IsConnected => _sp?.IsOpen ?? false;
        public IObservable<bool> ConnectionState { get; }

        /// <summary>
        ///     .SubscribeOn(new EventLoopScheduler())
        /// </summary>
        public IObservable<byte[]> Received { get; }

        public IObservable<string> Log { get; }

        public void Dispose()
        {
            _initDisposables?.Dispose();
            _connectionState?.Dispose();
            _received?.Dispose();
            _log?.Dispose();

            Stop();
        }

        public void Stop()
        {
            _controlDisposables?.Dispose();

            _sp?.Close();
            _sp?.Dispose();
        }

        /// <summary>
        ///     无返回值发送
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public async Task SendAsync(byte[] bytes)
        {
            await Task.Delay(_setting.SendDelay);

            _sp.Write(bytes, 0, bytes.Length);
        }

        /// <summary>
        ///     在超时时长内,根据模板自动截取返回值
        /// </summary>
        /// <param name="hex"></param>
        /// <param name="timeoutMs">ms</param>
        /// <param name="template"></param>
        /// <param name="sepBegin"></param>
        /// <param name="sepEnd"></param>
        /// <returns></returns>
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
                await Task.Delay(_setting.SendDelay);

                //var now = DateTimeOffset.Now;
                //var task = _received
                //    .SkipUntil(now)
                //    .TakeUntil(now.AddMilliseconds(timeoutMs))
                //    .Select(d => d.ByteToHex())
                //    .Scan((acc, r) => acc + r)
                //    .Select(r => r.TryGetTemplateMatchResults(template, sepBegin, sepEnd).FirstOrDefault())
                //    .ToTask();

                //var bytes = hex.HexToByte();
                //_sp.Write(bytes, 0, bytes.Length);

                //return await task;

                var task = _received
                    .Select(p => p.ByteToHex())
                    .TryGetTemplateMatchResult(template, timeoutMs, sepBegin, sepEnd);

                var bytes = hex.HexToByte();
                _sp.Write(bytes, 0, bytes.Length);

                return await task;
            }
            catch (Exception ex)
            {
                _log.OnNext($"Send Error: {ex.Message}");
                return string.Empty;
            }
        }

        /// <summary>
        ///     发送后等待固定时长后获取返回值, 需后续手动截取
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="waitMs"></param>
        /// <returns></returns>
        public async Task<byte[]> SendAsync(byte[] bytes, int waitMs)
        {
            try
            {
                await Task.Delay(_setting.SendDelay);

                var now = DateTimeOffset.Now;
                var task = _received
                    .SkipUntil(now)
                    .TakeUntil(now.AddMilliseconds(waitMs))
                    .Aggregate((x, y) => x.Concat(y).ToArray())
                    .ToTask();

                _sp.Write(bytes, 0, bytes.Length);

                return await task;
            }
            catch (Exception ex)
            {
                _log.OnNext($"Send Error: {ex.Message}");
                return Array.Empty<byte>();
            }
        }

        public void Start(SerialPortSetting setting, Action<object> config = null)
        {
            _setting = setting;

            Stop();

            if (_setting == default)
                return;

            if (!_setting.Enabled)
                return;

            _controlDisposables = new CompositeDisposable();

            _sp = new SP(
                _setting.PortName,
                _setting.BaudRate,
                _setting.Parity,
                _setting.DataBits,
                _setting.StopBits
            )
            {
                ReadTimeout = 500,
                WriteTimeout = 500
            };

            config?.Invoke(_sp);

            Observable
                .FromEventPattern(_sp, nameof(SP.DataReceived))
                .SubscribeOn(new EventLoopScheduler())
                .Subscribe(b =>
                {
                    try
                    {
                        var bs = new byte[_sp.BytesToRead];
                        _sp.Read(bs, 0, bs.Length);
                        _received.OnNext(bs);
                    }
                    catch
                    {
                        // ignored
                    }
                })
                .DisposeWith(_controlDisposables);

            if (_setting.AutoConnectEnabled)
            {
                Observable
                    .Interval(TimeSpan.FromMilliseconds(_setting.AutoConnectInterval))
                    .Where(_ => !IsConnected)
                    .SubscribeOn(TaskPoolScheduler.Default)
                    .Subscribe(s =>
                    {
                        try
                        {
                            _sp.Open();
                            _sp.DiscardInBuffer();
                            _sp.DiscardOutBuffer();
                        }
                        catch (FileNotFoundException)
                        {
                            _log.OnNext($"找不到串口{_sp.PortName}.");
                        }
                        catch (UnauthorizedAccessException)
                        {
                            _log.OnNext($"串口{_sp.PortName}已占用.");
                        }
                        catch (Exception e)
                        {
                            _log.OnNext(e.Message);
                        }
                    })
                    .DisposeWith(_controlDisposables);
            }
            else
            {
                _sp.Open();
                _sp.DiscardInBuffer();
                _sp.DiscardOutBuffer();
            }
        }

        public void Send(byte[] bytes)
        {
            _sp.Write(bytes, 0, bytes.Length);
        }

        public string[] GetPortNames()
        {
            return SP.GetPortNames();
        }
    }
}