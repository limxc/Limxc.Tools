﻿using System;
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
        protected Subject<bool> _connectionState;
        protected CompositeDisposable _disposables;
        protected Subject<CommContext> _history;
        protected Subject<byte[]> _received;

        protected ProtocolBase()
        {
            _disposables = new CompositeDisposable();

            _connectionState = new Subject<bool>();
            _received = new Subject<byte[]>();
            _history = new Subject<CommContext>();

            ConnectionState = Observable.Defer(() => _connectionState.AsObservable().Publish().RefCount());
            Received = Observable.Defer(() => _received.AsObservable().Publish().RefCount());
            History = Observable.Defer(() => _history.AsObservable().FindResponse(Received).Publish().RefCount());
        }


        public abstract bool IsConnected { get; }

        public IObservable<bool> ConnectionState { get; protected set; }
        public IObservable<byte[]> Received { get; protected set; }
        public IObservable<CommContext> History { get; protected set; }
        public abstract void Init(params object[] pars);

        public virtual Task<bool> OpenAsync()
        {
            throw new NotImplementedException();
        }

        public virtual Task<bool> CloseAsync()
        {
            throw new NotImplementedException();
        }

        public virtual Task<bool> SendAsync(CommContext context)
        {
            throw new NotImplementedException();
        }


        public virtual Task<bool> SendAsync(byte[] bytes)
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