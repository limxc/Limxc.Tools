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
using Limxc.Tools.SerialPort.Interfaces;
using Ports = System.IO.Ports.SerialPort;

namespace Limxc.Tools.SerialPort
{
    public class SerialPortService : ISerialPortService
    {
        private readonly Subject<bool> _connectionState = new Subject<bool>();
        private readonly CompositeDisposable _initDisposables = new CompositeDisposable();
        private readonly Subject<string> _log = new Subject<string>();
        private readonly Subject<byte[]> _received = new Subject<byte[]>();

        private CompositeDisposable _controlDisposables = new CompositeDisposable();
        private SerialPortSettingBase _serialPortSettingBase;

        private Ports _sp;

        protected SerialPortService(IObservable<SerialPortSettingBase> serialPortSetting)
        {
            serialPortSetting
                .DistinctUntilChanged()
                .Throttle(TimeSpan.FromSeconds(1))
                //.Where(p => GetPortNames().Contains(p.PortName))
                .SubscribeOn(NewThreadScheduler.Default)
                .Subscribe(p =>
                {
                    try
                    {
                        _serialPortSettingBase = p;
                        Start();
                    }
                    catch (Exception e)
                    {
                        _log.OnNext(e.Message);
                    }
                }).DisposeWith(_initDisposables);

            Observable
                .Interval(TimeSpan.FromSeconds(1))
                .Select(_ => IsConnected)
                .SubscribeOn(NewThreadScheduler.Default)
                .Subscribe(s => _connectionState.OnNext(s))
                .DisposeWith(_initDisposables);

            ConnectionState = Observable.Defer(() =>
                _connectionState.StartWith(false).AsObservable().Publish().RefCount());
            Received = Observable.Defer(() => _received.AsObservable().Publish().RefCount());
            Log = Observable.Defer(() => _log.AsObservable().Publish().RefCount());
        }

        public bool IsConnected => _sp?.IsOpen ?? false;
        public IObservable<bool> ConnectionState { get; }
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

        public void Start()
        {
            Stop();

            if (_serialPortSettingBase == default)
                return;

            _controlDisposables = new CompositeDisposable();

            _sp = new Ports(_serialPortSettingBase.PortName, _serialPortSettingBase.BaudRate,
                    _serialPortSettingBase.Parity, _serialPortSettingBase.DataBits, _serialPortSettingBase.StopBits)
                { ReadTimeout = 500, WriteTimeout = 500 };

            Observable
                .FromEventPattern(_sp, nameof(Ports.DataReceived))
                .SubscribeOn(NewThreadScheduler.Default)
                .Subscribe(b =>
                {
                    var bs = new byte[_sp.BytesToRead];
                    _sp.Read(bs, 0, bs.Length);
                    _received.OnNext(bs);
                })
                .DisposeWith(_controlDisposables);

            Observable
                .Interval(TimeSpan.FromMilliseconds(_serialPortSettingBase.AutoConnectInterval))
                .Where(_ => !IsConnected)
                .SubscribeOn(NewThreadScheduler.Default)
                .Subscribe(s =>
                {
                    try
                    {
                        _sp.Open();
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

        public void Stop()
        {
            _controlDisposables?.Dispose();

            _sp?.Close();
            _sp?.Dispose();
        }

        /// <summary>
        ///     无返回值发送
        /// </summary>
        /// <param name="hex"></param>
        /// <returns></returns>
        public async Task SendAsync(string hex)
        {
            await Task.Delay(_serialPortSettingBase.SendDelay);

            var bytes = hex.HexToByte();
            _sp.Write(bytes, 0, bytes.Length);
        }

        /// <summary>
        ///     在超时时长内,根据模板自动截取返回值
        /// </summary>
        /// <param name="hex"></param>
        /// <param name="timeoutMs">ms</param>
        /// <param name="template"></param>
        /// <param name="sep"></param>
        /// <returns></returns>
        public async Task<string> SendAsync(string hex, int timeoutMs, string template, char sep = '$')
        {
            try
            {
                await Task.Delay(_serialPortSettingBase.SendDelay);

                var now = DateTimeOffset.Now;
                var task = _received
                    .SkipUntil(now)
                    .TakeUntil(now.AddMilliseconds(timeoutMs))
                    .Select(d => d.ByteToHex())
                    .Scan((acc, r) => acc + r)
                    .FirstOrDefaultAsync(r => template.IsTemplateMatch(r, sep, false))
                    .Select(r => template.TryGetTemplateMatchResult(r, sep))
                    .ToTask();

                var bytes = hex.HexToByte();
                _sp.Write(bytes, 0, bytes.Length);

                return await task;
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        ///     发送后等待固定时长后获取返回值, 需后续手动截取
        /// </summary>
        /// <param name="hex"></param>
        /// <param name="waitMs"></param>
        /// <returns></returns>
        public async Task<byte[]> SendAsync(string hex, int waitMs)
        {
            try
            {
                await Task.Delay(_serialPortSettingBase.SendDelay);

                var now = DateTimeOffset.Now;
                var task = _received.SkipUntil(now).TakeUntil(now.AddMilliseconds(waitMs))
                    .Aggregate((x, y) => x.Concat(y).ToArray()).ToTask();

                var bytes = hex.HexToByte();
                _sp.Write(bytes, 0, bytes.Length);

                return await task;
            }
            catch
            {
                return Array.Empty<byte>();
            }
        }

        public string[] GetPortNames()
        {
            return Ports.GetPortNames();
        }
    }
}