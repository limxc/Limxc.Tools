using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Limxc.Tools.Entities.DevComm;

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

        public Task<bool> CloseAsync()
        {
            return Task.FromResult(false);
        }

        public Task<bool> OpenAsync()
        {
            return Task.FromResult(false);
        }

        public Task<bool> SendAsync(CPContext cmd)
        {
            return Task.FromResult(false);
        }

        public Task<bool> SendAsync(byte[] bytes)
        {
            return Task.FromResult(false);
        }

        public void Dispose()
        {
        }
    }
}