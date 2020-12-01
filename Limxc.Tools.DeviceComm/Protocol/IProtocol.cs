using Limxc.Tools.DeviceComm.Entities;
using System;
using System.Threading.Tasks;

namespace Limxc.Tools.DeviceComm.Protocol
{
    public interface IProtocol<TConnectionState> : IDisposable
    {
        IObservable<TConnectionState> ConnectionState { get; }
        IObservable<byte[]> Received { get; }
        IObservable<CPContext> History { get; }

        Task<bool> SendAsync(CPContext cmd);

        Task<bool> OpenAsync();

        Task<bool> CloseAsync();
    }

    public interface ISerialPortProtocol : IProtocol<bool>
    {
    }

    public interface ITcpProtocol : IProtocol<ProcotolConnectionState>
    {
    }
}