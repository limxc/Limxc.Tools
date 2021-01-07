using GodSharp.SerialPort;
using Limxc.Tools.DeviceComm.Entities;
using Limxc.Tools.DeviceComm.Extensions;
using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;

namespace Limxc.Tools.DeviceComm.Protocol
{
    public class SerialPortProtocol_GS : IProtocol
    {
        private GodSerialPort _sp;

        private ISubject<CPContext> _msg;
        private readonly string _portName;
        private readonly int _baudRate;

        public SerialPortProtocol_GS(string portName, int baudRate)
        {
            _portName = portName;
            _baudRate = baudRate;

            _msg = new Subject<CPContext>();

            _sp = new GodSerialPort(_portName, _baudRate, 0);

            ConnectionState = Observable.Defer(() =>
            {
                return Observable
                            .Interval(TimeSpan.FromSeconds(0.1))
                            .Select(_ => _sp?.IsOpen ?? false)
                            .StartWith(false)
                            .DistinctUntilChanged();
            });

            Received = Observable
                            .Create<byte[]>(x =>
                            {
                                _sp.UseDataReceived(true, (gs, data) =>
                                {
                                    if (data?.Length > 0)
                                        x.OnNext(data);
                                });
                                return Disposable.Empty;
                            })
                            .Retry()
                            .Publish()
                            .RefCount()
                             //.Debug("receive")
                             ;

            History = Observable.Defer(() =>
            {
                return _msg.AsObservable()
                            //.Debug("send")
                            .FindResponse(Received)
                            //.Debug("prase received")
                            ;
            });
        }

        public IObservable<bool> ConnectionState { get; }
        public IObservable<byte[]> Received { get; }
        public IObservable<CPContext> History { get; }

        public void CleanUp()
        {
            _msg?.OnCompleted();
            _msg = null;

            _sp?.Close();
            _sp = null;
        }

        public Task<bool> SendAsync(CPContext context)
        {
            var cmdStr = context.Command.Build();
            _sp.WriteHexString(cmdStr);

            context.SendTime = DateTime.Now;
            _msg.OnNext(context);

            return Task.FromResult(true);
        }

        public Task<bool> SendAsync(byte[] bytes)
        {
            _sp.Write(bytes);

            return Task.FromResult(true);
        }

        public Task<bool> OpenAsync()
        {
            return Task.FromResult(_sp.Open());
        }

        public Task<bool> CloseAsync()
        {
            return Task.FromResult(_sp.Close());
        }
    }
}