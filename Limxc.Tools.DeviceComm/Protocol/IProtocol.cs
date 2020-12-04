using Limxc.Tools.DeviceComm.Entities;
using System;
using System.Threading.Tasks;

namespace Limxc.Tools.DeviceComm.Protocol
{
    public interface IPortProtocol : IDisposable
    {
        IObservable<bool> ConnectionState { get; }
        IObservable<byte[]> Received { get; }
        IObservable<CPContext> History { get; }

        Task<bool> SendAsync(CPContext cmd);

        Task<bool> OpenAsync();

        Task<bool> CloseAsync();
    }
}