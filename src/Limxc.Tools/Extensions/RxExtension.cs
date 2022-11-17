using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;

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
                Console.WriteLine(
                    $"****** {msg ?? "Rx"} @ {DateTime.Now:mm:ss fff} : {p} ******"));
        }

        public static IDisposable SubscribeToConsole<T>(this IObservable<T> obs)
        {
            return obs
                .Subscribe(
                    x => Console.WriteLine($"OnNext @ {DateTime.Now:mm:ss fff} : {x}"),
                    e => Console.WriteLine($"OnError @ {DateTime.Now:mm:ss fff} : {e.Message}"),
                    () => Console.WriteLine($"OnComplete @ {DateTime.Now:mm:ss fff}")
                );
        }

        #region BufferUntil

        public static IObservable<T[]> BufferUntil<T>(this IObservable<T[]> obs, T[] startWith, T[] endWith,
            int timeout, T[] def = null) where T : IEquatable<T>
        {
            return Observable.Create<T[]>(o =>
            {
                var dis = new CompositeDisposable();
                var bs = new List<T>();

                var startFound = false;
                var elapsedTime = 0;

                obs.Subscribe(s =>
                    {
                        elapsedTime = 0;
                        if (startFound || s.SequenceEqual(startWith))
                        {
                            startFound = true;
                            bs.AddRange(s);
                            if (s.SequenceEqual(endWith))
                            {
                                o.OnNext(bs.ToArray());
                                startFound = false;
                                bs.Clear();
                            }
                        }
                    }, o.OnError, o.OnCompleted)
                    .DisposeWith(dis);

                Observable.Interval(TimeSpan.FromMilliseconds(1)).Subscribe(_ =>
                {
                    elapsedTime++;
                    if (elapsedTime > timeout)
                    {
                        if (def != null)
                            o.OnNext(def);
                        startFound = false;
                        bs.Clear();
                        elapsedTime = 0;
                    }
                }).DisposeWith(dis);

                return dis;
            });
        }

        public static IObservable<T[]> BufferUntil<T>(this IObservable<T[]> obs, T[] startWith, int length, int timeout,
            T[] def = null) where T : IEquatable<T>
        {
            return Observable.Create<T[]>(o =>
            {
                var dis = new CompositeDisposable();
                var bs = new List<T>();

                var startFound = false;
                var elapsedTime = 0;

                obs.Subscribe(s =>
                    {
                        elapsedTime = 0;
                        if (startFound || s.SequenceEqual(startWith))
                        {
                            startFound = true;
                            bs.AddRange(s);
                            if (bs.Count >= length)
                            {
                                o.OnNext(bs.ToArray());
                                startFound = false;
                                bs.Clear();
                            }
                        }
                    }, o.OnError, o.OnCompleted)
                    .DisposeWith(dis);

                Observable.Interval(TimeSpan.FromMilliseconds(1)).Subscribe(_ =>
                {
                    elapsedTime++;
                    if (elapsedTime > timeout)
                    {
                        if (def != null)
                            o.OnNext(def);
                        startFound = false;
                        bs.Clear();
                        elapsedTime = 0;
                    }
                }).DisposeWith(dis);

                return dis;
            });
        }

        public static IObservable<T[]> BufferUntil<T>(this IObservable<T> obs, T startWith, T endWith, int timeout,
            T[] def = null) where T : IEquatable<T>
        {
            return Observable.Create<T[]>(o =>
            {
                var dis = new CompositeDisposable();
                var list = new List<T>();

                var startFound = false;
                var elapsedTime = 0;

                obs.Subscribe(s =>
                    {
                        elapsedTime = 0;
                        if (startFound || s.Equals(startWith))
                        {
                            startFound = true;
                            list.Add(s);
                            if (s.Equals(endWith))
                            {
                                o.OnNext(list.ToArray());
                                startFound = false;
                                list.Clear();
                            }
                        }
                    }, o.OnError, o.OnCompleted)
                    .DisposeWith(dis);

                Observable.Interval(TimeSpan.FromMilliseconds(1)).Subscribe(_ =>
                {
                    elapsedTime++;
                    if (elapsedTime > timeout)
                    {
                        if (def != null)
                            o.OnNext(def);
                        startFound = false;
                        list.Clear();
                        elapsedTime = 0;
                    }
                }).DisposeWith(dis);

                return dis;
            });
        }

        #endregion BufferUntil

        #region Bucket

        public static IObservable<T[]> Bucket<T>(this IObservable<T> source, TimeSpan size, TimeSpan shift)
        {
            return Observable.Create<T[]>(o =>
            {
                var dis = new CompositeDisposable();
                var queue = new ConcurrentQueue<Timestamped<T>>();

                source
                    .Timestamp()
                    .Subscribe(s => queue.Enqueue(s), o.OnError, o.OnCompleted)
                    .DisposeWith(dis);

                Observable.Interval(shift).Subscribe(_ =>
                {
                    var startTime = DateTimeOffset.UtcNow.Subtract(size);

                    var res = queue
                        .Where(p => p.Timestamp >= startTime)
                        .Select(p => p.Value)
                        .ToArray();

                    while (queue.Any(p => p.Timestamp < startTime)) queue.TryDequeue(out var _);
                    o.OnNext(res);
                }).DisposeWith(dis);

                return dis;
            });
        }

        public static IObservable<T[]> Bucket<T>(this IObservable<T> source, int number)
        {
            return Observable.Create<T[]>(o =>
            {
                var queue = new ConcurrentQueue<T>();

                return source.Subscribe(s =>
                {
                    queue.Enqueue(s);
                    if (queue.Count > number) queue.TryDequeue(out _);
                    o.OnNext(queue.ToArray());
                }, o.OnError, o.OnCompleted);
            });
        }

        #endregion Bucket
    }
}