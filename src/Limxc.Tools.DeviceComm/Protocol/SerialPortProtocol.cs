using System;
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
        private int _baudRate;
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
        public SerialPortProtocol(int autoConnectInterval)
        {
            var isConnecting = false;
            _autoConnectDisposable = Observable
                .Interval(TimeSpan.FromMilliseconds(autoConnectInterval))
                .Where(_ => !string.IsNullOrWhiteSpace(_portName))
                .Select(_ => SerialPort.GetPortNames())
                .Where(ports => ports.Contains(_portName, StringComparer.CurrentCultureIgnoreCase)) //port found
                .ObserveOn(NewThreadScheduler.Default)
                .Subscribe(async _ =>
                {
                    if (!isConnecting && !IsConnected)
                    {
                        isConnecting = true;
                        try
                        {
                            await OpenAsync();
                        }
                        catch
                        {
                            // ignored
                        }

                        isConnecting = false;
                    }
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
            _portName = portName;
            _baudRate = baudRate;

            _disposables?.Dispose();
            _disposables = new CompositeDisposable();

            _sp?.Close();
            _sp = new SerialPort(_portName, _baudRate, 0) {ReadTimeout = 500, WriteTimeout = 500};

            Observable
                .Interval(TimeSpan.FromSeconds(0.1))
                .Select(_ => _sp?.IsOpen ?? false)
                .StartWith(false)
                .DistinctUntilChanged()
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
        }


        public override async Task<bool> SendAsync(CommContext context)
        {
            var cmdStr = context.Command.Build();
            var bs = cmdStr.HexToByte();

            context.SendTime = DateTime.Now;
            _history.OnNext(context);

            _sp.Write(bs, 0, bs.Length);
            await Task.Delay(50);
            return true;
        }

        public override Task<bool> SendAsync(byte[] bytes)
        {
            _sp.Write(bytes, 0, bytes.Length);

            return Task.FromResult(true);
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