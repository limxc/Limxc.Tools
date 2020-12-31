using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
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
            if (Debugger.IsAttached)
                return obs.Do(p =>
                {
                    System.Diagnostics.Debug.WriteLine($"****** {msg ?? "Rx"} @ {DateTime.Now:mm:ss fff} : {p.ToString()} ******");
                });
            else
                return obs;
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

        public static IObservable<byte[]> BufferUntil(this IObservable<byte[]> obs, byte[] startWith, byte[] endWith, int timeOut)
            => Observable.Create<byte[]>(o =>
            {
                var dis = new CompositeDisposable();
                var bs = new List<byte>();

                var startFound = false;
                var elapsedTime = 0;

                var sub = obs.Subscribe(s =>
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
                }).DisposeWith(dis);

                Observable.Interval(TimeSpan.FromMilliseconds(1)).Subscribe(_ =>
                {
                    elapsedTime++;
                    if (elapsedTime > timeOut)
                    {
                        startFound = false;
                        bs.Clear();
                        elapsedTime = 0;
                    }
                }).DisposeWith(dis);

                return dis;
            });

        public static IObservable<byte[]> BufferUntil(this IObservable<byte[]> obs, byte[] startWith, int length, int timeOut)
            => Observable.Create<byte[]>(o =>
            {
                var dis = new CompositeDisposable();
                var bs = new List<byte>();

                var startFound = false;
                var elapsedTime = 0;

                var sub = obs.Subscribe(s =>
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
                }).DisposeWith(dis);

                Observable.Interval(TimeSpan.FromMilliseconds(1)).Subscribe(_ =>
                {
                    elapsedTime++;
                    if (elapsedTime > timeOut)
                    {
                        startFound = false;
                        bs.Clear();
                        elapsedTime = 0;
                    }
                }).DisposeWith(dis);

                return dis;
            });

        public static IObservable<string> BufferUntil(this IObservable<char> obs, char startWith, char endWith, int timeOut)
            => Observable.Create<string>(o =>
            {
                var dis = new CompositeDisposable();
                var str = "";

                var startFound = false;
                var elapsedTime = 0;

                var sub = obs.Subscribe(s =>
                {
                    elapsedTime = 0;
                    if (startFound || s == startWith)
                    {
                        startFound = true;
                        str += s;
                        if (s == endWith)
                        {
                            o.OnNext(str);
                            startFound = false;
                            str = "";
                        }
                    }
                }).DisposeWith(dis);

                Observable.Interval(TimeSpan.FromMilliseconds(1)).Subscribe(_ =>
                {
                    elapsedTime++;
                    if (elapsedTime > timeOut)
                    {
                        startFound = false;
                        str = "";
                        elapsedTime = 0;
                    }
                }).DisposeWith(dis);

                return dis;
            });

        public static IObservable<string> BufferUntil(this IObservable<char> obs, char startWith, char endWith, string defString, int timeOut)
            => Observable.Create<string>(o =>
            {
                var dis = new CompositeDisposable();
                var str = "";

                var startFound = false;
                var elapsedTime = 0;

                var sub = obs.Subscribe(s =>
                {
                    elapsedTime = 0;
                    if (startFound || s == startWith)
                    {
                        startFound = true;
                        str += s;
                        if (s == endWith)
                        {
                            o.OnNext(str);
                            startFound = false;
                            str = "";
                        }
                    }
                }).DisposeWith(dis);

                Observable.Interval(TimeSpan.FromMilliseconds(1)).Subscribe(_ =>
                {
                    elapsedTime++;
                    if (elapsedTime > timeOut)
                    {
                        o.OnNext(defString);
                        startFound = false;
                        str = "";
                        elapsedTime = 0;
                    }
                }).DisposeWith(dis);

                return dis;
            });

        #endregion BufferUntil

        #region Bucket

        public static IObservable<T[]> Bucket<T>(this IObservable<T> source, TimeSpan size, TimeSpan shift)
        {
            return Observable.Create<T[]>(o =>
            {
                var dis = new CompositeDisposable();
                var list = new ConcurrentBag<Timestamped<T>>();

                source
                    .Timestamp()
                    .Subscribe(s => list.Add(s), () => o.OnCompleted())
                    .DisposeWith(dis);

                Observable.Interval(shift).Subscribe(_ =>
                {
                    var res = list.Where(p => p.Timestamp > DateTimeOffset.UtcNow.Subtract(size))
                            .Select(p => p.Value)
                            .ToArray();
                    Array.Reverse(res);
                    o.OnNext(res);
                }).DisposeWith(dis);

                return dis;
            });
        }

        public static IObservable<T[]> Bucket<T>(this IObservable<T> source, int number)
        {
            return source.Publish(s =>
            {
                return Observable.Defer
                    (() => s
                            .Scan(new NumberBucket<T>(number, new T[0]), (pack, obj) => pack.Push(obj))
                            .Where(p => p.Accumulated.Any())
                            .Select(p => p.Accumulated.ToArray())
                    );
            });
        }

        private class NumberBucket<T>
        {
            private int _count;

            public NumberBucket(int count, IEnumerable<T> accumulated)
            {
                _count = count;
                Accumulated = accumulated;
            }

            public IEnumerable<T> Accumulated { get; private set; }

            public NumberBucket<T> Push(T b)
            {
                Accumulated = Accumulated.Concat(new[] { b });
                var ac = Accumulated.Count();
                if (ac > _count)
                {
                    Accumulated = Accumulated.Skip(ac - _count);
                }
                return new NumberBucket<T>(_count, Accumulated);
            }
        }

        #endregion Bucket
    }
}