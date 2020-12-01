using Limxc.Tools.DeviceComm.Entities;
using System;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace Limxc.Tools.DeviceComm.Protocol
{
    public interface IProtocol<TConnectionState> : IDisposable
    {
        IObservable<TConnectionState> ConnectionState { get; }
        IObservable<byte[]> Received { get; }
        IObservable<CPContext> History { get; }

        Task<bool> Send(CPContext cmd);

        Task<bool> Connect();

        Task<bool> Disconnect();
    }

    public interface ISerialPortProtocol : IProtocol<bool>
    {
    }

    public interface ITcpProtocol : IProtocol<ProcotolConnectionState>
    {
    }
}