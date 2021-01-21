using Limxc.Tools.Entities.DevComm;
using Limxc.Tools.Extensions;
using Limxc.Tools.Extensions.DevComm;
using System;
using System.IO.Ports;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;

namespace Limxc.Tools.DeviceComm.Protocol
{
    public class SerialPortProtocol : IProtocol
    {
        private SerialPort _sp;

        private ISubject<bool> _connectionState;
        private ISubject<byte[]> _received;
        private ISubject<CPContext> _history;

        private CompositeDisposable _disposables;

        private bool disposedValue;

        public SerialPortProtocol()
        {
            _connectionState = new Subject<bool>();
            _received = new Subject<byte[]>();
            _history = new Subject<CPContext>();

            ConnectionState = Observable.Defer(() => _connectionState.AsObservable().Publish().RefCount());
            Received = Observable.Defer(() => _received.AsObservable().Publish().RefCount());
            History = Observable.Defer(() => _history.AsObservable().FindResponse(Received).Publish().RefCount());
        }

        public void Init(string portName, int baudRate)
        {
            _disposables?.Dispose();
            _disposables = new CompositeDisposable();

            _sp?.Close();
            _sp = new SerialPort(portName, baudRate, Parity.None);
            //_sp.ReadTimeout = 500;
            //_sp.WriteTimeout = 500;
            //_sp.DtrEnable = true;
            //_sp.RtsEnable = true;

            Observable
                .Interval(TimeSpan.FromSeconds(0.1))
                .Select(_ => _sp?.IsOpen ?? false)
                .StartWith(false)
                .DistinctUntilChanged()
                .SubscribeOn(NewThreadScheduler.Default)
                .Subscribe(s => _connectionState.OnNext(s))
                .DisposeWith(_disposables);

            Observable.FromEventPattern<SerialDataReceivedEventHandler, SerialDataReceivedEventArgs>(h => _sp.DataReceived += h, h => _sp.DataReceived -= h)
                .Select(_ => _sp.Encoding.GetBytes(_sp.ReadExisting()))
                .SubscribeOn(NewThreadScheduler.Default)
                .Subscribe(b => _received.OnNext(b))
                .DisposeWith(_disposables);
        }

        public bool IsConnected => _sp?.IsOpen ?? false;
        public IObservable<bool> ConnectionState { get; }
        public IObservable<byte[]> Received { get; }
        public IObservable<CPContext> History { get; }

        public Task<bool> SendAsync(CPContext context)
        {
            var cmdStr = context.Command.Build();
            _sp.Write(cmdStr);

            context.SendTime = DateTime.Now;
            _history.OnNext(context);

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

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: 释放托管状态(托管对象)
                    _connectionState?.OnCompleted();
                    _received?.OnCompleted();
                    _history?.OnCompleted();
                }
                // TODO: 释放未托管的资源(未托管的对象)并替代终结器
                _sp?.Close();
                _disposables?.Dispose();
                // TODO: 将大型字段设置为 null
                _connectionState = null;
                _received = null;
                _history = null;
                _sp = null;
                disposedValue = true;
            }
        }

        // TODO: 仅当“Dispose(bool disposing)”拥有用于释放未托管资源的代码时才替代终结器
        ~SerialPortProtocol()
        {
            // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}