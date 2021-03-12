using System;
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
        private GodSerialPort _sp;

        public override bool IsConnected => _sp?.IsOpen ?? false;

        public override void Init(params object[] pars)
        {
            var portName = (string) pars[0];
            var baudRate = (int) pars[1];
            Init(portName, baudRate);
        }

        public void Init(string portName, int baudRate)
        {
            _disposables?.Dispose();
            _disposables = new CompositeDisposable();

            _sp?.Close();
            _sp = new GodSerialPort(portName, baudRate, 0);

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
            return Task.FromResult(_sp.Close());
        }

        protected override void Dispose(bool disposing)
        {
            _sp?.Close();
            base.Dispose(disposing);
        }
    }
}