using System;
using System.Threading.Tasks;
using Limxc.Tools.Entities.Communication;
using Limxc.Tools.Extensions.Communication;

namespace Limxc.Tools.DeviceComm.Protocol
{
    public class ProtocolSimulator : ProtocolBase
    {
        private readonly int _lostSimulateInterval;
        private readonly int _sendDelayMs;

        public ProtocolSimulator(int lostSimulateInterval = 0, int sendDelayMs = 500)
        {
            _sendDelayMs = sendDelayMs;
            _lostSimulateInterval = lostSimulateInterval;

            _connectionState.OnNext(true);
        }

        public override bool IsConnected => true;

        public override void Init(params object[] pars)
        {
        }

        public override Task<bool> OpenAsync()
        {
            return Task.FromResult(true);
        }

        public override Task<bool> CloseAsync()
        {
            return Task.FromResult(true);
        }


        public override async Task<bool> SendAsync(CommContext context)
        {
            context.SendTime = DateTime.Now;
            _history.OnNext(context);

            if (_lostSimulateInterval > 0 && DateTime.Now.Second % _lostSimulateInterval == 0) //模拟失败
                return true;

            await Task.Delay(_sendDelayMs);
            _received.OnNext(context.Command.Build().HexToByte()); //发什么回什么

            return true;
        }

        public override async Task<bool> SendAsync(byte[] bytes)
        {
            if (_lostSimulateInterval > 0 && DateTime.Now.Second % _lostSimulateInterval == 0) //模拟失败
                return true;

            await Task.Delay(_sendDelayMs);
            _received.OnNext(bytes); //发什么回什么

            return true;
        }
    }
}