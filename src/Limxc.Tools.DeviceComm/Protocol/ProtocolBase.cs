using System;
using System.Collections.Generic;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks;
using Limxc.Tools.Entities.Communication;
using Limxc.Tools.Extensions;
using Limxc.Tools.Extensions.Communication;

// ReSharper disable InconsistentNaming

namespace Limxc.Tools.DeviceComm.Protocol
{
    public abstract class ProtocolBase : IDisposable
    {
        protected ISubject<bool> _connectionState;

        protected CompositeDisposable _disposables;
        protected ISubject<CommContext> _history;
        protected ISubject<byte[]> _received;

        protected ProtocolBase()
        {
            _disposables = new CompositeDisposable();

            _connectionState = new Subject<bool>().DisposeWith(_disposables);
            _received = new Subject<byte[]>().DisposeWith(_disposables);
            _history = new Subject<CommContext>().DisposeWith(_disposables);

            ConnectionState = Observable.Defer(() => _connectionState.AsObservable().Publish().RefCount());
            Received = Observable.Defer(() => _received.AsObservable().Publish().RefCount());
            History = Observable.Defer(() => _history.AsObservable().FindResponse(Received).Publish().RefCount());
        }


        public abstract bool IsConnected { get; }

        public IObservable<bool> ConnectionState { get; protected set; }
        public IObservable<byte[]> Received { get; protected set; }
        public IObservable<CommContext> History { get; protected set; }

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

        /// <summary>
        ///     解析返回值
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="waitMs"></param>
        /// <returns></returns>
        public async Task<byte[]> SendAsync(byte[] bytes, int waitMs)
        {
            var now = DateTimeOffset.Now;
            if (await SendAsync(bytes))
                return await Received.SkipUntil(now).TakeUntil(now.AddMilliseconds(waitMs)).ToTask();
            return new byte[0];
        }

        /// <summary>
        ///     Send解析任务完成通知
        /// </summary>
        /// <param name="context"></param>
        /// <param name="schedulerRunTime">rx处理时间</param>
        /// <returns></returns>
        public async Task WaitingSendResult(CommTaskContext context,
            int schedulerRunTime = 100)
        {
            var state = 0; //等待中..

            var dis = History
                .TakeUntil(DateTimeOffset.Now.AddMilliseconds(context.Timeout + schedulerRunTime))
                .FirstOrDefaultAsync(p => ((CommTaskContext) p).Id == context.Id)
                .ObserveOn(TaskPoolScheduler.Default)
                .Subscribe(p =>
                {
                    if (p == null)
                        state = 1; //返回值丢失
                    else
                        state = 2; //有返回值
                });

            await SendAsync(context).ConfigureAwait(false);

            while (state == 0) await Task.Delay(10).ConfigureAwait(false);

            dis.Dispose();

            if (state == 1)
                throw new Exception($"Send Result Lost. {nameof(CommTaskContext.Id)}:{context.Id}");
        }

        /// <summary>
        ///     任务队列执行
        /// </summary>
        /// <param name="queue"></param>
        /// <param name="token"></param>
        /// <param name="schedulerRunTime">rx处理时间</param>
        /// <returns></returns>
        public async Task ExecQueue(List<CommTaskContext> queue, CancellationToken token,
            int schedulerRunTime = 100)
        {
            foreach (var task in queue)
                while (!token.IsCancellationRequested && task.State != CommContextState.Success &&
                       task.State != CommContextState.NoNeed && task.RemainTimes > 0)
                {
                    task.RemainTimes--;
                    await WaitingSendResult(task, schedulerRunTime).ConfigureAwait(false);
                }
        }

        #region Dispose

        protected virtual void Dispose(bool disposing)
        {
            if (disposing) _disposables?.Dispose();
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