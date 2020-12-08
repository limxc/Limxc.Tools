using System;
using System.Threading;
using System.Threading.Tasks;

namespace Limxc.Tools.Abstractions
{
    public interface ICommClientService
    {
        IObservable<bool> Connected { get; }

        Task Start(string clientId, string serverIp, int port);

        Task Stop();

        void CleanUp();

        Task Pub(string topic, string payload, CancellationToken token);

        Task Pub<T>(string topic, T payload, CancellationToken token);

        Task<string> RpcPub(string methodName, string msg, int timeoutSeconds = 15);

        Task<TRst> RpcPub<TMsg, TRst>(string methodName, TMsg msg, int timeoutSeconds = 15);

        IDisposable RpcSub(string methodName, Func<string, Task<string>> action);

        IDisposable RpcSub<TMsg, TRst>(string methodName, Func<TMsg, Task<TRst>> action);

        IObservable<string> Sub(string topic);

        IObservable<T> Sub<T>(string topic);
    }
}