using Limxc.Tools.Pipeline.Context;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Limxc.Tools.Pipeline.Builder
{
    public class PipeBuilder<T> : IPipeBuilder<T> where T : class
    {
        private readonly List<Func<PipeHandlerDel<T>, PipeHandlerDel<T>>> _handlers = new List<Func<PipeHandlerDel<T>, PipeHandlerDel<T>>>();
        private PipeHandlerDel<T> finalHandler;

        public PipeBuilder<T> Build()
        {
            _handlers.Reverse();
            finalHandler = (context, token) =>
            {
                PipeHandlerDel<T> next = (c, t) => Task.CompletedTask;

                foreach (var middleware in _handlers)
                    next = middleware(next);

                return next(context, token);
            };
            return this;
        }

        public IPipeBuilder<T> Use(Func<PipeHandlerDel<T>, PipeHandlerDel<T>> handler)
        {
            _handlers.Add(handler);
            return this;
        }

        public IPipeBuilder<T> Use(Func<T, Task> handler, string descForSnapshot = null)
        {
            Func<PipeHandlerDel<T>, PipeHandlerDel<T>> warp = next =>
            {
                return async (context, token) =>
                    {
                        if (handler != null && !token.IsCancellationRequested)
                        {
                            await handler(context.Body);
                            if (!string.IsNullOrWhiteSpace(descForSnapshot))
                                context.AddSnapshot(descForSnapshot);
                        }
                        await next(context, token).ConfigureAwait(false);
                    };
            };

            _handlers.Add(warp);
            return this;
        }

        public IPipeBuilder<T> Use(Action<T> handler, string descForSnapshot = null)
        {
            Func<PipeHandlerDel<T>, PipeHandlerDel<T>> warp = next =>
            {
                return async (context, token) =>
                {
                    if (handler != null && !token.IsCancellationRequested)
                    {
                        handler(context.Body);
                        if (!string.IsNullOrWhiteSpace(descForSnapshot))
                            context.AddSnapshot(descForSnapshot);
                    }
                    await next(context, token).ConfigureAwait(false);
                };
            };

            _handlers.Add(warp);
            return this;
        }

        public IPipeBuilder<T> Use(Func<T, CancellationToken, Task> handler, string descForSnapshot = null)
        {
            Func<PipeHandlerDel<T>, PipeHandlerDel<T>> warp = next =>
            {
                return async (context, token) =>
                {
                    if (handler != null && !token.IsCancellationRequested)
                    {
                        await handler(context.Body, token);
                        if (!string.IsNullOrWhiteSpace(descForSnapshot) && !token.IsCancellationRequested)
                            context.AddSnapshot(descForSnapshot);
                    }
                    await next(context, token).ConfigureAwait(false);
                };
            };

            _handlers.Add(warp);
            return this;
        }

        public IPipeBuilder<T> Use(Action<T, CancellationToken> handler, string descForSnapshot = null)
        {
            Func<PipeHandlerDel<T>, PipeHandlerDel<T>> warp = next =>
            {
                return async (context, token) =>
                {
                    if (handler != null && !token.IsCancellationRequested)
                    {
                        handler(context.Body, token);
                        if (!string.IsNullOrWhiteSpace(descForSnapshot) && !token.IsCancellationRequested)
                            context.AddSnapshot(descForSnapshot);
                    }
                    await next(context, token).ConfigureAwait(false);
                };
            };

            _handlers.Add(warp);
            return this;
        }

        public async Task<PipeContext<T>> RunAsync(T obj, CancellationToken token)
        {
            var context = new PipeContext<T>(obj);
            context.AddSnapshot("original");
            await finalHandler(context, token);
            return context;
        }

        public Task<PipeContext<T>> RunAsync(T obj)
        {
            return RunAsync(obj, CancellationToken.None);
        }
    }
}