﻿using System;
using System.IO.Ports;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using Limxc.Tools.Extensions;
using Limxc.Tools.Extensions.Communication;

namespace Limxc.Tools.DeviceComm.Utils
{
    /// <summary>
    ///     SerialPort自动连接管理
    /// </summary>
    public class SerialPortConnector : IDisposable
    {
        private readonly IDisposable _autoConnectDisposable;
        private readonly Subject<bool> _connectionState = new Subject<bool>();
        private readonly Subject<byte[]> _received = new Subject<byte[]>();
        private readonly int _sendInterval;
        private int _baudRate;
        private CompositeDisposable _disposables = new CompositeDisposable();
        private string _portName;
        private SerialPort _sp;

        public SerialPortConnector(int autoConnectInterval = 100, int sendInterval = 50)
        {
            _sendInterval = sendInterval;
            var isConnecting = false;
            _autoConnectDisposable = Observable
                .Interval(TimeSpan.FromMilliseconds(autoConnectInterval))
                .Where(_ => !string.IsNullOrWhiteSpace(_portName))
                .Select(_ => SerialPort.GetPortNames())
                .Where(ports => ports.Contains(_portName, StringComparer.CurrentCultureIgnoreCase)) //port found
                .ObserveOn(NewThreadScheduler.Default)
                .Subscribe(_ =>
                {
                    if (!isConnecting && !IsConnected)
                    {
                        isConnecting = true;
                        try
                        {
                            _sp.Open();
                            _sp.DiscardInBuffer();
                            _sp.DiscardOutBuffer();
                        }
                        catch
                        {
                            // ignored
                        }

                        isConnecting = false;
                    }
                });

            ConnectionState = Observable.Defer(() => _connectionState.AsObservable().Publish().RefCount());
            Received = Observable.Defer(() => _received.AsObservable().Publish().RefCount());
        }

        public bool IsConnected => _sp?.IsOpen ?? false;
        public IObservable<bool> ConnectionState { get; }
        public IObservable<byte[]> Received { get; }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Init(string portName, int baudRate)
        {
            _portName = portName;
            _baudRate = baudRate;

            _disposables?.Dispose();
            _disposables = new CompositeDisposable();

            _sp?.Close();
            _sp = new SerialPort(_portName, _baudRate, 0) {ReadTimeout = 500, WriteTimeout = 500};

            Observable
                .FromEventPattern(_sp, nameof(SerialPort.DataReceived))
                .SubscribeOn(NewThreadScheduler.Default)
                .Subscribe(b =>
                {
                    var bs = new byte[_sp.BytesToRead];
                    _sp.Read(bs, 0, bs.Length);
                    _received.OnNext(bs);
                })
                .DisposeWith(_disposables);

            Observable
                .Interval(TimeSpan.FromSeconds(0.1))
                .Select(_ => IsConnected)
                .StartWith(false)
                .DistinctUntilChanged()
                .SubscribeOn(NewThreadScheduler.Default)
                .Subscribe(s => _connectionState.OnNext(s))
                .DisposeWith(_disposables);
        }

        /// <summary>
        ///     无返回值发送
        /// </summary>
        /// <param name="hex"></param>
        /// <returns></returns>
        public async Task SendAsync(string hex)
        {
            await Task.Delay(_sendInterval);

            var bytes = hex.HexToByte();
            _sp.Write(bytes, 0, bytes.Length);
        }

        /// <summary>
        ///     在超时时长内,根据模板自动截取返回值
        /// </summary>
        /// <param name="hex"></param>
        /// <param name="timeOut"></param>
        /// <param name="template"></param>
        /// <param name="sep"></param>
        /// <returns></returns>
        public async Task<string> SendAsync(string hex, int timeOut, string template, char sep = '$')
        {
            try
            {
                await Task.Delay(_sendInterval);

                var now = DateTimeOffset.Now;
                var task = _received
                    .SkipUntil(now)
                    .TakeUntil(now.AddMilliseconds(timeOut))
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
                await Task.Delay(_sendInterval);

                var now = DateTimeOffset.Now;
                var task = _received.SkipUntil(now).TakeUntil(now.AddMilliseconds(waitMs))
                    .Aggregate((x, y) => x.Concat(y).ToArray()).ToTask();

                var bytes = hex.HexToByte();
                _sp.Write(bytes, 0, bytes.Length);

                return await task;
            }
            catch
            {
                return new byte[0];
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _autoConnectDisposable?.Dispose();
                _received?.Dispose();
                _connectionState?.Dispose();
                _disposables?.Dispose();
                _sp?.Dispose();
            }
        }
    }
}