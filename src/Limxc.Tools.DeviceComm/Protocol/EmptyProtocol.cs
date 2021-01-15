using Limxc.Tools.Entities.DevComm;
using System;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace Limxc.Tools.DeviceComm.Protocol
{
    public class EmptyProtocol : IProtocol
    {
        public EmptyProtocol()
        {
            ConnectionState = Observable.Empty<bool>();
            Received = Observable.Empty<byte[]>();
            History = Observable.Empty<CPContext>();
        }

        public IObservable<bool> ConnectionState { get; }

        public IObservable<byte[]> Received { get; }

        public IObservable<CPContext> History { get; }

        public void CleanUp()
        {
        }

        public Task<bool> CloseAsync() => Task.FromResult(false);

        public Task<bool> OpenAsync() => Task.FromResult(false);

        public Task<bool> SendAsync(CPContext cmd) => Task.FromResult(false);

        public Task<bool> SendAsync(byte[] bytes) => Task.FromResult(false);

        public void Dispose()
        {
        }
    }
}