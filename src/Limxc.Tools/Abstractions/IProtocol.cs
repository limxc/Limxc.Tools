using System;
using System.Threading.Tasks;
using Limxc.Tools.Entities.DevComm;

namespace Limxc.Tools.DeviceComm.Protocol
{
    public interface IProtocol : IDisposable
    {
        IObservable<bool> ConnectionState { get; }
        IObservable<byte[]> Received { get; }
        IObservable<CPContext> History { get; }

        Task<bool> SendAsync(CPContext cmd);

        Task<bool> SendAsync(byte[] bytes);

        Task<bool> OpenAsync();

        Task<bool> CloseAsync();
    }
}