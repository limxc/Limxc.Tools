using Limxc.Tools.DeviceComm.Entities;
using System;
using System.Threading.Tasks;

namespace Limxc.Tools.DeviceComm.Protocol
{
    public interface IProtocol : IDisposable
    {
        IObservable<bool> IsConnected { get; }
        IObservable<string> Received { get; }
        IObservable<CPContext> History { get; }

        Task<bool> Send(CPContext cmd);

        Task<bool> Connect(string portName, int baudRate);

        Task<bool> Disconnect();
    }
}