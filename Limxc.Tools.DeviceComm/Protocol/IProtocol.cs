using Limxc.Tools.DeviceComm.Entities;
using System;
using System.Threading.Tasks;

namespace Limxc.Tools.DeviceComm.Protocol
{
    public interface IProtocol : IDisposable
    {
        IObservable<CPContext> History { get; }

        IObservable<bool> IsConnected { get; }
        IObservable<string> Received { get; }

        Task<bool> Send(CPContext cmd);

        Task<bool> Start(string portName, int baudRate);

        Task<bool> Stop();
    }
}