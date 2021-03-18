using System;
using System.IO.Ports;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using GodSharp.SerialPort;
using Limxc.Tools.Entities.Communication;
using Limxc.Tools.Extensions;

namespace Limxc.Tools.DeviceComm.Protocol
{
    public class SerialPortProtocol : ProtocolBase
    {
        private readonly IDisposable _autoConnectDisposable;
        private int _baudRate;
        private string _portName;
        private GodSerialPort _sp;

        /// <summary>
        ///     Manual Connect
        /// </summary>
        public SerialPortProtocol()
        {
        }

        /// <summary>
        ///     Auto Connect
        /// </summary>
        /// <param name="autoConnectInterval">Milliseconds</param>
        public SerialPortProtocol(int autoConnectInterval = 1000)
        {
            var isConnecting = false;
            _autoConnectDisposable = Observable
                .Interval(TimeSpan.FromMilliseconds(autoConnectInterval))
                .Where(_ => !string.IsNullOrWhiteSpace(_portName))
                .Select(_ => SerialPort.GetPortNames())
                .Select(ports => ports.Contains(_portName, StringComparer.CurrentCultureIgnoreCase)) //port found
                .Where(p => p && !IsConnected) //need to try connect
                .ObserveOn(NewThreadScheduler.Default)
                .Subscribe(async _ =>
                {
                    if (!isConnecting)
                    {
                        isConnecting = true;
                        await OpenAsync();
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
            _sp = new GodSerialPort(_portName, _baudRate, 0);

            Observable
                .Interval(TimeSpan.FromSeconds(0.1))
                .Select(_ => _sp?.IsOpen ?? false)
                .StartWith(false)
                .DistinctUntilChanged()
                .SubscribeOn(NewThreadScheduler.Default)
                .Subscribe(s => _connectionState.OnNext(s))
                .DisposeWith(_disposables);

            Observable
                .Create<byte[]>(x =>
                {
                    _sp.UseDataReceived(true, (gs, data) =>
                    {
                        if (data?.Length > 0)
                            x.OnNext(data);
                    });
                    return Disposable.Empty;
                })
                .SubscribeOn(NewThreadScheduler.Default)
                .Subscribe(b => _received.OnNext(b))
                .DisposeWith(_disposables);
        }


        public override Task<bool> SendAsync(CommContext context)
        {
            var cmdStr = context.Command.Build();
            _sp.WriteHexString(cmdStr);

            context.SendTime = DateTime.Now;
            _history.OnNext(context);

            return Task.FromResult(true);
        }

        public override Task<bool> SendAsync(byte[] bytes)
        {
            _sp.Write(bytes);

            return Task.FromResult(true);
        }


        public override Task<bool> OpenAsync()
        {
            return Task.FromResult(_sp.Open());
        }

        public override Task<bool> CloseAsync()
        {
            return Task.FromResult(_sp?.Close() ?? false);
        }

        protected override void Dispose(bool disposing)
        {
            _autoConnectDisposable?.Dispose();
            _sp?.Close();
            base.Dispose(disposing);
        }
    }
}