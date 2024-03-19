using System;
using System.Threading;
using System.Threading.Tasks;
using Limxc.Tools.Pipeline.Context;

namespace Limxc.Tools.Pipeline.Builder
{
    public interface IPipeBuilder<T>
        where T : class
    {
        PipeBuilder<T> Build();

        IPipeBuilder<T> UseSnapshotCloner(Func<T, T> cloner);

        IPipeBuilder<T> Use(Func<PipeHandlerDel<T>, PipeHandlerDel<T>> handler);

        IPipeBuilder<T> Use(Func<T, Task> handler, string descForSnapshot = null);

        IPipeBuilder<T> Use(Action<T> handler, string descForSnapshot = null);

        IPipeBuilder<T> Use(
            Func<T, CancellationToken, Task> handler,
            string descForSnapshot = null
        );

        IPipeBuilder<T> Use(Action<T, CancellationToken> handler, string descForSnapshot = null);

        Task<PipeContext<T>> RunAsync(T obj);

        Task<PipeContext<T>> RunAsync(T obj, CancellationToken token);
    }

    public delegate Task PipeHandlerDel<T>(PipeContext<T> context, CancellationToken token)
        where T : class;
}