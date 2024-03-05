using System;
using System.Threading;
using System.Threading.Tasks;

namespace Limxc.Tools.Contract.Interfaces
{
    public interface IMessageBusService : IDisposable
    {
        Task Pub<T>(T payload, string topic = null, CancellationToken token = default);
        IObservable<T> Sub<T>(string topic = null);
    }
}
