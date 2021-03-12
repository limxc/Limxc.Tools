using System.Reactive.Linq;
using System.Threading.Tasks;
using Limxc.Tools.Entities.Communication;

namespace Limxc.Tools.DeviceComm.Protocol
{
    public class EmptyProtocol : ProtocolBase
    {
        public EmptyProtocol()
        {
            ConnectionState = Observable.Empty<bool>();
            Received = Observable.Empty<byte[]>();
            History = Observable.Empty<CommContext>();
        }

        public override bool IsConnected => true;

        public override Task<bool> CloseAsync()
        {
            return Task.FromResult(false);
        }

        public override void Init(params object[] pars)
        {
        }

        public override Task<bool> OpenAsync()
        {
            return Task.FromResult(false);
        }

        public override Task<bool> SendAsync(CommContext cmd)
        {
            return Task.FromResult(false);
        }

        public override Task<bool> SendAsync(byte[] bytes)
        {
            return Task.FromResult(false);
        }
    }
}