using Limxc.Tools.DeviceComm.Entities;
using Limxc.Tools.DeviceComm.Extensions;
using Limxc.Tools.DeviceComm.Protocol;
using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;

namespace UnitTest.TestUtils
{
    public class ProtocolSimulator : IProtocol
    {
        private readonly int _sendDelayMs;
        private readonly int _lostSimulateInterval;
        private bool _isConnected;

        private Subject<byte[]> _received;

        private Subject<CPContext> _msg;
        private CompositeDisposable _disposables;

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

        public void CleanUp()
        {
            _disposables?.Dispose();
            _msg?.Dispose();
            _received?.Dispose();
        }

        public Task<bool> CloseAsync()
        {
            _isConnected = false;
            _disposables?.Dispose();

            return Task.FromResult(true);
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

        public async Task<bool> SendAsync(CPContext cmd)
        {
            cmd.SendTime = DateTime.Now;
            _msg.OnNext(cmd);

            await Task.Delay(_sendDelayMs);

            if (_lostSimulateInterval > 0 && DateTime.Now.Second % _lostSimulateInterval == 0)//模拟失败
                return true;

            //if(cmd.Timeout != 0 && !string.IsNullOrWhiteSpace(cmd.Response.Template))
                _received.OnNext(cmd.ToCommand().ToByte());//发什么回什么

            return true;
        }
    }
}