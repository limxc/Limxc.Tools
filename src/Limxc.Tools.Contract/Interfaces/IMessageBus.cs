using System;
using System.Threading;
using System.Threading.Tasks;

namespace Limxc.Tools.Contract.Interfaces
{
    public interface IMessageBus : IDisposable
    {
        Task Pub<T>(T payload, string topic, CancellationToken token);
        IObservable<T> Sub<T>(string topic);
    }
}