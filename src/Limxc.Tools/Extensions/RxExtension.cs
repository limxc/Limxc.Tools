using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace Limxc.Tools.Extensions
{
    public static class RxExtension
    {
        public static T DisposeWith<T>(this T disposable, ICollection<IDisposable> container)
            where T : IDisposable
        {
            container.Add(disposable);
            return disposable;
        }

        public static IObservable<T> Debug<T>(this IObservable<T> obs, string msg = "")
        {
            return obs.Do(p =>
                Console.WriteLine($"****** {msg ?? "Rx"} @ {DateTime.Now:mm:ss fff} : {p} ******")
            );
        }

        public static IDisposable SubscribeToConsole<T>(this IObservable<T> obs)
        {
            return obs.Subscribe(
                x => Console.WriteLine($"OnNext @ {DateTime.Now:mm:ss fff} : {x}"),
                e => Console.WriteLine($"OnError @ {DateTime.Now:mm:ss fff} : {e.Message}"),
                () => Console.WriteLine($"OnComplete @ {DateTime.Now:mm:ss fff}")
            );
        }

        #region Bucket

        /// <summary>
        ///     Scan+Buffer
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="size"></param>
        /// <param name="shift"></param>
        /// <returns></returns>
        public static IObservable<T[]> Bucket<T>(
            this IObservable<T> source,
            TimeSpan size,
            TimeSpan shift
        )
        {
            return Observable.Create<T[]>(o =>
            {
                var dis = new CompositeDisposable();
                var queue = new ConcurrentQueue<Timestamped<T>>();

                source
                    .Timestamp()
                    .Subscribe(s => queue.Enqueue(s), o.OnError, o.OnCompleted)
                    .DisposeWith(dis);

                Observable
                    .Interval(shift)
                    .Subscribe(_ =>
                    {
                        var startTime = DateTimeOffset.UtcNow.Subtract(size);

                        var res = queue
                            .Where(p => p.Timestamp >= startTime)
                            .Select(p => p.Value)
                            .ToArray();

                        while (queue.Any(p => p.Timestamp < startTime))
                            queue.TryDequeue(out var _);

                        o.OnNext(res);
                    })
                    .DisposeWith(dis);

                return dis;
            });
        }

        /// <summary>
        ///     Scan+Buffer
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="number"></param>
        /// <returns></returns>
        public static IObservable<T[]> Bucket<T>(this IObservable<T> source, int number)
        {
            return Observable.Create<T[]>(o =>
            {
                var queue = new ConcurrentQueue<T>();

                return source.Subscribe(
                    s =>
                    {
                        queue.Enqueue(s);
                        if (queue.Count > number)
                            queue.TryDequeue(out _);
                        o.OnNext(queue.ToArray());
                    },
                    o.OnError,
                    o.OnCompleted
                );
            });
        }

        #endregion Bucket

        #region Subscribe

        public static IObservable<TR> CallAsync<T, TR>(
            this IObservable<T> source,
            Func<T, Task<TR>> onNextAsync
        )
        {
            return source.Select(t => Observable.FromAsync(() => onNextAsync(t))).Concat();
        }

        public static IObservable<Unit> CallAsync<T>(
            this IObservable<T> source,
            Func<T, Task> onNextAsync
        )
        {
            return source.Select(t => Observable.FromAsync(() => onNextAsync(t))).Concat();
        }

        public static IObservable<TR> CallAsyncConcurrent<T, TR>(
            this IObservable<T> source,
            Func<T, Task<TR>> onNextAsync
        )
        {
            return source.Select(t => Observable.FromAsync(() => onNextAsync(t))).Merge();
        }

        public static IObservable<Unit> CallAsyncConcurrent<T>(
            this IObservable<T> source,
            Func<T, Task> onNextAsync
        )
        {
            return source.Select(t => Observable.FromAsync(() => onNextAsync(t))).Merge();
        }

        public static IObservable<TR> CallAsyncConcurrent<T, TR>(
            this IObservable<T> source,
            Func<T, Task<TR>> onNextAsync,
            int maxConcurrent
        )
        {
            return source.Select(t => Observable.FromAsync(() => onNextAsync(t))).Merge(maxConcurrent);
        }

        public static IObservable<Unit> CallAsyncConcurrent<T>(
            this IObservable<T> source,
            Func<T, Task> onNextAsync,
            int maxConcurrent
        )
        {
            return source.Select(t => Observable.FromAsync(() => onNextAsync(t))).Merge(maxConcurrent);
        }

        #endregion
    }
}