using System;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Limxc.Tools.DeviceComm.Abstractions;
using Limxc.Tools.Entities.Communication;
using Limxc.Tools.Extensions.Communication;

// ReSharper disable InconsistentNaming 
namespace Limxc.Tools.DeviceComm.Protocol
{
    public abstract class ProtocolBase : IProtocol
    {
        protected readonly Subject<bool> _connectionState;
        protected readonly Subject<CommContext> _history;
        protected readonly Subject<byte[]> _received;
        protected CompositeDisposable _disposables;

        protected ProtocolBase()
        {
            _disposables = new CompositeDisposable();

            _connectionState = new Subject<bool>();
            _received = new Subject<byte[]>();
            _history = new Subject<CommContext>();

            ConnectionState = Observable.Defer(() => _connectionState.AsObservable().Publish().RefCount());
            Received = Observable.Defer(() => _received.AsObservable().Publish().RefCount());
            History = Observable.Defer(() =>
                _history.AsObservable().FindResponse(Received).ObserveOn(NewThreadScheduler.Default)
                    .Publish().RefCount());
        }


        public abstract bool IsConnected { get; }

        public IObservable<bool> ConnectionState { get; protected set; }
        public IObservable<byte[]> Received { get; protected set; }
        public IObservable<CommContext> History { get; protected set; }
        public abstract void Init(params object[] pars);

        public abstract Task<bool> OpenAsync();

        public abstract Task<bool> CloseAsync();

        public abstract Task<bool> SendAsync(byte[] bytes);

        public virtual Task<bool> SendAsync(CommContext context)
        {
            throw new NotImplementedException();
        }

        #region Dispose

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _disposables?.Dispose();
                _history?.Dispose();
                _connectionState?.Dispose();
                _received?.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~ProtocolBase()
        {
            Dispose(false);
        }

        #endregion
    }
}