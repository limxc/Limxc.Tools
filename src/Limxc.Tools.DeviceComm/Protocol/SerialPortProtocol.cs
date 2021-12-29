using System;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Limxc.Tools.Entities.Communication;
using Limxc.Tools.Extensions;
using Limxc.Tools.Extensions.Communication;

namespace Limxc.Tools.DeviceComm.Protocol
{
    public class SerialPortProtocol : ProtocolBase
    {
        private readonly IDisposable _autoConnectDisposable;
        private readonly int _autoConnectInterval;
        private readonly int _sendDelay;
        private int _baudRate;
        private bool _isBusy;
        private string _portName;
        private SerialPort _sp;

        /// <summary>
        ///     Manual Connect
        /// </summary>
        public SerialPortProtocol()
        {
        }

        /// <summary>
        ///     Auto Connect
        /// </summary>
        /// <param name="autoConnectInterval">50~1000ms</param>
        /// <param name="sendDelay">10+</param>
        public SerialPortProtocol(int autoConnectInterval = 100, int sendDelay = 50)
        {
            _autoConnectInterval = autoConnectInterval;
            _sendDelay = sendDelay;

            _autoConnectDisposable = Observable
                .Interval(TimeSpan.FromMilliseconds(_autoConnectInterval))
                .Where(_ => !string.IsNullOrWhiteSpace(_portName))
                .Select(_ => SerialPort.GetPortNames())
                .Where(ports => ports.Contains(_portName, StringComparer.CurrentCultureIgnoreCase)) //port found
                .Where(_ => _sp?.IsOpen == false && !_isBusy)
                .SubscribeOn(NewThreadScheduler.Default)
                .Subscribe(async _ =>
                {
                    _isBusy = true;
                    try
                    {
                        await OpenAsync();
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e.Message);
                    }

                    _isBusy = false;
                });
        }

        public override bool IsConnected => _sp?.IsOpen ?? false;

        public override void Init(params object[] pars)
        {
            var portName = (string) pars[0];
            var baudRate = (int) pars[1];
            Init(portName, baudRate);
        }

        public void Init(string portName, int baudRate)
        {
            _isBusy = true;

            _portName = portName;
            _baudRate = baudRate;

            _disposables?.Dispose();
            _disposables = new CompositeDisposable();

            _sp?.Close();
            _sp = new SerialPort(_portName, _baudRate, 0) {ReadTimeout = 500, WriteTimeout = 500};

            Observable
                .Interval(TimeSpan.FromMilliseconds(_autoConnectInterval))
                .Select(_ => IsConnected)
                .SubscribeOn(NewThreadScheduler.Default)
                .Subscribe(s => _connectionState.OnNext(s))
                .DisposeWith(_disposables);

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

            _isBusy = false;
        }


        public override async Task<bool> SendAsync(CommContext context)
        {
            await Task.Delay(_sendDelay);

            var cmdStr = context.Command.Build();
            var bs = cmdStr.HexToByte();

            context.SendTime = DateTime.Now;
            _history.OnNext(context);

            _sp.Write(bs, 0, bs.Length);

            return true;
        }

        public override async Task<bool> SendAsync(byte[] bytes)
        {
            await Task.Delay(_sendDelay);

            _sp.Write(bytes, 0, bytes.Length);

            return true;
        }


        public override Task<bool> OpenAsync()
        {
            _sp.Open();
            _sp.DiscardInBuffer();
            _sp.DiscardOutBuffer();
            return Task.FromResult(true);
        }

        public override Task<bool> CloseAsync()
        {
            _sp?.Close();
            return Task.FromResult(true);
        }

        protected override void Dispose(bool disposing)
        {
            _autoConnectDisposable?.Dispose();
            _sp?.Close();
            base.Dispose(disposing);
        }
    }
}