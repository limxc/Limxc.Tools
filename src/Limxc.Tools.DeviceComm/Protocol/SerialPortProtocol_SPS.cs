using Limxc.Tools.DeviceComm.Entities;
using Limxc.Tools.DeviceComm.Extensions;
using Limxc.Tools.DeviceComm.Utils;
using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;

namespace Limxc.Tools.DeviceComm.Protocol
{
    public class SerialPortProtocol_SPS : IProtocol
    {
        private SerialPortStreamHelper _sp;
        private readonly string _portName;
        private readonly int _baudRate;

        private ISubject<CPContext> _msg;

        public SerialPortProtocol_SPS(string portName, int baudRate)
        {
            _portName = portName;
            _baudRate = baudRate;

            _msg = new Subject<CPContext>();

            _sp = new SerialPortStreamHelper();

            ConnectionState = Observable
                            .Interval(TimeSpan.FromSeconds(0.1))
                            .Select(_ => _sp?.IsOpen ?? false)
                            .StartWith(false)
                            .DistinctUntilChanged();

            Received = Observable
                            .FromEventPattern<byte[]>(h => _sp.ReceivedEvent += h, h => _sp.ReceivedEvent -= h)
                            .Where(p => p.EventArgs?.Length > 0)
                            .Select(p => p.EventArgs)
                            .Retry()
                            .Publish()
                            .RefCount()
                            //.Debug("receive")
                            ;

            History = _msg.AsObservable()
                            //.Debug("send")
                            .FindResponse(Received, b => b.ToHexStrFromChar())
                            //.Debug("prase received")
                            ;
        }

        public IObservable<bool> ConnectionState { get; }
        public IObservable<byte[]> Received { get; }
        public IObservable<CPContext> History { get; }

        public void CleanUp()
        {
            _msg?.OnCompleted();
            _msg = null;

            _sp?.Close();
            _sp?.CleanUp();
            _sp = null;
        }

        public Task<bool> SendAsync(CPContext context)
        {
            var cmdStr = context.Command.Build();
            _sp.Write(cmdStr);

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
            return Task.FromResult(_sp.Open(_portName, _baudRate));
        }

        public Task<bool> CloseAsync()
        {
            _sp.Close();
            return Task.FromResult(true);
        }
    }
}