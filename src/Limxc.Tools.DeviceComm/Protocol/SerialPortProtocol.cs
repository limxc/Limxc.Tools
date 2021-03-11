using System;
using System.IO.Ports;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Limxc.Tools.Entities.DevComm;
using Limxc.Tools.Extensions.DevComm;

namespace Limxc.Tools.DeviceComm.Protocol
{
    public class SerialPortProtocol : IProtocol
    {
        private readonly int _baudRate;
        private readonly string _portName;

        private ISubject<CPContext> _msg;
        private SerialPort _sp;
        private bool disposedValue;

        public SerialPortProtocol(string portName, int baudRate)
        {
            _portName = portName;
            _baudRate = baudRate;

            _msg = new Subject<CPContext>();

            _sp = new SerialPort(_portName, _baudRate, 0);

            ConnectionState = Observable.Defer(() =>
            {
                return Observable
                    .Interval(TimeSpan.FromSeconds(0.1))
                    .Select(_ => _sp?.IsOpen ?? false)
                    .StartWith(false)
                    .DistinctUntilChanged();
            });

            Received = Observable.FromEventPattern(_sp, nameof(SerialPort.DataReceived))
                    .Select(_ => _sp.Encoding.GetBytes(_sp.ReadExisting()))
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
            _sp.Write(bytes, 0, bytes.Length);

            return Task.FromResult(true);
        }

        public Task<bool> OpenAsync()
        {
            _sp.Open();
            return Task.FromResult(true);
        }

        public Task<bool> CloseAsync()
        {
            _sp.Close();
            return Task.FromResult(true);
        }

        public void Dispose()
        {
            // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                    // TODO: 释放托管状态(托管对象)
                    _msg?.OnCompleted();
                // TODO: 释放未托管的资源(未托管的对象)并替代终结器
                _sp?.Close();
                // TODO: 将大型字段设置为 null
                _msg = null;
                _sp = null;
                disposedValue = true;
            }
        }

        // TODO: 仅当“Dispose(bool disposing)”拥有用于释放未托管资源的代码时才替代终结器
        ~SerialPortProtocol()
        {
            // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
            Dispose(false);
        }
    }
}