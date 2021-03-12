using System;
using System.Threading.Tasks;
using Limxc.Tools.Entities.Communication;

namespace Limxc.Tools.DeviceComm.Abstractions
{
    public interface IProtocol : IDisposable
    {
        IObservable<bool> ConnectionState { get; }
        IObservable<byte[]> Received { get; }
        IObservable<CommContext> History { get; }
        Task<bool> OpenAsync();

        Task<bool> CloseAsync();

        Task<bool> SendAsync(CommContext context);

        Task<bool> SendAsync(byte[] bytes);

        void Init(params object[] pars);
    }
}