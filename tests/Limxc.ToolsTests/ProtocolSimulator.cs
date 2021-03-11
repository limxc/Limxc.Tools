using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Limxc.Tools.DeviceComm.Protocol;
using Limxc.Tools.Entities.DevComm;
using Limxc.Tools.Extensions.DevComm;

namespace Limxc.Tools.Tests
{
    public class ProtocolSimulator : IProtocol
    {
        private readonly int _lostSimulateInterval;
        private readonly int _sendDelayMs;
        private CompositeDisposable _disposables;
        private bool _isConnected;

        private readonly Subject<CPContext> _msg;

        private readonly Subject<byte[]> _received;

        public ProtocolSimulator(int lostSimulateInterval = 0, int sendDelayMs = 500)
        {
            _sendDelayMs = sendDelayMs;
            _lostSimulateInterval = lostSimulateInterval;

            _received = new Subject<byte[]>();
            _msg = new Subject<CPContext>();

            ConnectionState = Observable
                .Interval(TimeSpan.FromSeconds(0.1))
                .Select(_ => _isConnected)
                .StartWith(false)
                .DistinctUntilChanged()
                .Retry()
                .Publish();

            Received = _received.AsObservable()
                    .Retry()
                    .Publish()
                ;

            History = _msg.AsObservable()
                    //.Debug("send")
                    .FindResponse(Received)
                    //.Debug("prase received")
                    .Publish()
                ;
        }

        public IObservable<bool> ConnectionState { get; }

        public IObservable<byte[]> Received { get; }

        public IObservable<CPContext> History { get; }

        public Task<bool> CloseAsync()
        {
            _isConnected = false;
            _disposables?.Dispose();

            return Task.FromResult(true);
        }

        public void Dispose()
        {
            _disposables?.Dispose();
            _msg?.Dispose();
            _received?.Dispose();
        }

        public Task<bool> OpenAsync()
        {
            _isConnected = true;

            _disposables = new CompositeDisposable();

            (ConnectionState as IConnectableObservable<bool>).Connect().DisposeWith(_disposables);
            (Received as IConnectableObservable<byte[]>).Connect().DisposeWith(_disposables);
            (History as IConnectableObservable<CPContext>).Connect().DisposeWith(_disposables);

            return Task.FromResult(true);
        }

        public async Task<bool> SendAsync(CPContext context)
        {
            context.SendTime = DateTime.Now;
            _msg.OnNext(context);

            await Task.Delay(_sendDelayMs);

            if (_lostSimulateInterval > 0 && DateTime.Now.Second % _lostSimulateInterval == 0) //模拟失败
                return true;

            //if(cmd.Timeout != 0 && !string.IsNullOrWhiteSpace(cmd.Response.Template))
            _received.OnNext(context.Command.Build().ToByte()); //发什么回什么

            return true;
        }

        public async Task<bool> SendAsync(byte[] bytes)
        {
            await Task.Delay(_sendDelayMs);

            if (_lostSimulateInterval > 0 && DateTime.Now.Second % _lostSimulateInterval == 0) //模拟失败
                return true;

            _received.OnNext(bytes); //发什么回什么

            return true;
        }

        public async Task<bool> SendAsync(CPContext context, byte[] resp, int delay)
        {
            context.SendTime = DateTime.Now;
            _msg.OnNext(context);

            await Task.Delay(delay);

            if (_lostSimulateInterval > 0 && DateTime.Now.Second % _lostSimulateInterval == 0) //模拟失败
                return true;

            _received.OnNext(resp);

            return true;
        }

        public async Task<bool> SendAsync(byte[] bytes, byte[] resp, int delay)
        {
            await Task.Delay(delay);

            if (_lostSimulateInterval > 0 && DateTime.Now.Second % _lostSimulateInterval == 0) //模拟失败
                return true;

            _received.OnNext(resp); //发什么回什么

            return true;
        }
    }
}