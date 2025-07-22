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
        private const int MinReadBufferSizeUnit = 1024;
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
                .Subscribe(s => _connectionState.OnNext(s))
                .DisposeWith(_initDisposables);

            ConnectionState =
                Observable.Defer(() =>
                    _connectionState.StartWith(false).AsObservable().ObserveOn(new EventLoopScheduler()).Publish()
                        .RefCount());

            Received = Observable.Defer(() =>
                _received.AsObservable().ObserveOn(new EventLoopScheduler()).Publish().RefCount());

            Log = Observable.Defer(() => _log.AsObservable().ObserveOn(new EventLoopScheduler()).Publish().RefCount());
        }

        public bool IsConnected => _sp?.IsOpen ?? false;
        public IObservable<bool> ConnectionState { get; }

        public IObservable<byte[]> Received { get; }

        public IObservable<string> Log { get; }

        public virtual void Dispose()
        {
            _initDisposables?.Dispose();
            _connectionState?.Dispose();
            _received?.Dispose();
            _log?.Dispose();

            Stop();
        }

        /// <summary>
        /// </summary>
        /// <param name="setting"></param>
        /// <param name="configSerialPort"></param>
        public void Start(SerialPortSetting setting, Action<object> configSerialPort = null)
        {
            if (setting == null)
                throw new ArgumentNullException(nameof(setting));

            _setting = setting;

            Stop();
             
            _controlDisposables = new CompositeDisposable();

            _sp = new SP();

            if (_setting.Check(out var msg))
            {
                _sp.PortName = _setting.PortName;
                _sp.BaudRate = _setting.BaudRate;
            }
            else
            {
                _log.OnNext(msg);
            }

            configSerialPort?.Invoke(_sp);

            var readBufferSize = setting.ReadBufferSize;
            if (readBufferSize % MinReadBufferSizeUnit != 0)
                if (readBufferSize % 1024 != 0)
                    readBufferSize = 1024 * (readBufferSize / 1024 + 1);
            readBufferSize = readBufferSize <= 4096 ? 4096 : readBufferSize;

            _sp.ReadBufferSize = readBufferSize;
            var buffer = new byte[readBufferSize];

            Observable.Interval(TimeSpan.FromMilliseconds(1))
                .ObserveOn(new EventLoopScheduler())
                .Where(_ => IsConnected)
                .Subscribe(_ =>
                {
                    try
                    {
                        if (_sp.BytesToRead == 0) return;

                        var len = _sp.Read(buffer, 0, buffer.Length);

                        if (len == 0) return;

                        _received.OnNext(buffer.Take(len).ToArray());
                    }
                    catch (Exception ex)
                    {
                        _log.OnNext(ex.ToString());
                    }
                })
                .DisposeWith(_controlDisposables);

            if (_setting.AutoConnectEnabled)
            {
                Observable
                    .Interval(TimeSpan.FromMilliseconds(_setting.AutoConnectInterval))
                    .ObserveOn(new EventLoopScheduler())
                    .Where(_ => !IsConnected)
                    .Subscribe(s =>
                    {
                        try
                        {
                            _sp.PortName = _setting.PortName;
                            _sp.BaudRate = _setting.BaudRate;
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

        public void Stop()
        {
            _controlDisposables?.Dispose();

            _sp?.Close();
            _sp?.Dispose();
        }

        public void Send(byte[] bytes)
        {
            _sp.Write(bytes, 0, bytes.Length);
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


        public string[] GetPortNames()
        {
            return SP.GetPortNames();
        }
    }
}