using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Limxc.Tools.DeviceComm.Abstractions;
using Limxc.Tools.Entities.Communication;
using Limxc.Tools.Extensions.Communication;

namespace Limxc.Tools.DeviceComm.Protocol
{
    public class ProtocolSimulator : IProtocol
    {
        private readonly Subject<bool> _connectionState;
        private readonly bool _fakeData;
        private readonly Subject<CommContext> _history;

        private readonly int _lostInterval;
        private readonly Subject<byte[]> _received;
        private readonly int _sendDelay;

        private int _sendIndex;

        /// <summary>
        /// </summary>
        /// <param name="lostInterval">每n个丢失一个</param>
        /// <param name="sendDelay">发送延时ms</param>
        /// <param name="fakeData">是否使用假数据(false则发什么回什么)</param>
        public ProtocolSimulator(int lostInterval = 0, int sendDelay = 100, bool fakeData = false)
        {
            _sendDelay = sendDelay;
            _fakeData = fakeData;
            _lostInterval = lostInterval;

            _connectionState = new Subject<bool>();
            _received = new Subject<byte[]>();
            _history = new Subject<CommContext>();

            ConnectionState = _connectionState.StartWith(true).AsObservable().Publish().RefCount();
            Received =
                _received.AsObservable().Publish().RefCount();
            History =
                _history.AsObservable().FindResponse(Received).ObserveOn(NewThreadScheduler.Default)
                    .Publish().RefCount();
        }

        public bool IsConnected => true;

        public void Init(params object[] pars)
        {
        }

        public IObservable<bool> ConnectionState { get; }
        public IObservable<byte[]> Received { get; }
        public IObservable<CommContext> History { get; }

        public Task<bool> OpenAsync()
        {
            return Task.FromResult(true);
        }

        public Task<bool> CloseAsync()
        {
            return Task.FromResult(true);
        }

        public async Task<bool> SendAsync(CommContext context)
        {
            context.SendTime = DateTime.Now;
            _history.OnNext(context);
            _sendIndex++;

            if (_lostInterval > 0 && _sendIndex % _lostInterval == 0) //模拟失败
                return false;

            await Task.Delay(_sendDelay);

            if (_fakeData)
                _received.OnNext(context.Response.Template.SimulateResponse().HexToByte()); //根据响应模板生成
            else
                _received.OnNext(context.Command.Build().HexToByte()); //发什么回什么

            return true;
        }

        public async Task<bool> SendAsync(byte[] bytes)
        {
            if (_lostInterval > 0 && DateTime.Now.Second % _lostInterval == 0) //模拟失败
                return false;

            await Task.Delay(_sendDelay);

            _received.OnNext(bytes); //发什么回什么

            return true;
        }

        #region Dispose

        public void Dispose()
        {
            _connectionState?.Dispose();
            _history?.Dispose();
            _received?.Dispose();
        }

        #endregion
    }
}