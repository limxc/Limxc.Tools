using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace Limxc.Tools.Extensions.Communication
{
    public static class ParseExtension
    {
        #region 分隔符分包

        /// <summary>
        ///     分隔符分包
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="separator"></param>
        /// <returns></returns>
        public static IObservable<T[]> ParsePackage<T>(this IObservable<T> source, T separator)
            where T : IEquatable<T>
        {
            if (separator == null)
                throw new ArgumentException("separator is null.");

            return source.ParsePackage(new[] { separator });
        }

        /// <summary>
        ///     分隔符分包
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="separator"></param>
        /// <returns></returns>
        public static IObservable<T[]> ParsePackage<T>(this IObservable<T> source, T[] separator)
            where T : IEquatable<T>
        {
            if (separator == null || separator.Length == 0)
                throw new ArgumentException("Separator is empty.");

            return Observable.Create<T[]>(observer =>
            {
                var queue = new Queue<T>();
                var separatorLength = separator.Length;

                var disposable = source.Subscribe(b =>
                    {
                        queue.Enqueue(b);

                        if (queue.Count < separatorLength)
                            return;

                        if (queue.Skip(queue.Count - separatorLength).SequenceEqual(separator))
                        {
                            if (queue.Count > separatorLength)
                                observer.OnNext(queue.Take(queue.Count - separatorLength).ToArray());

                            queue.Clear();
                        }
                    },
                    observer.OnError,
                    () =>
                    {
                        if (queue.Count > 0)
                            observer.OnNext(queue.ToArray());
                        observer.OnCompleted();
                    });

                return disposable;
            });
        }

        /// <summary>
        ///     分隔符分包
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="separator"></param>
        /// <returns></returns>
        public static IObservable<T[]> ParsePackage<T>(this IObservable<IEnumerable<T>> source, params T[] separator)
            where T : IEquatable<T>
        {
            if (separator == null || separator.Length == 0)
                throw new ArgumentException("Separator is empty.");

            return Observable.Create<T[]>(observer =>
            {
                var queue = new Queue<T>();
                var separatorLength = separator.Length;

                var disposable = source.Subscribe(bytes =>
                    {
                        foreach (var b in bytes)
                        {
                            queue.Enqueue(b);

                            if (queue.Count < separatorLength)
                                continue;

                            if (queue.Skip(queue.Count - separatorLength).SequenceEqual(separator))
                            {
                                if (queue.Count > separatorLength)
                                    observer.OnNext(queue.Take(queue.Count - separatorLength).ToArray());

                                queue.Clear();
                            }
                        }
                    },
                    observer.OnError,
                    () =>
                    {
                        if (queue.Count > 0)
                            observer.OnNext(queue.ToArray());
                        observer.OnCompleted();
                    });

                return disposable;
            });
        }

        #endregion

        #region 包头包尾分包

        /// <summary>
        ///     包头包尾分包
        /// </summary>
        /// <param name="source"></param>
        /// <param name="bom"></param>
        /// <param name="eom"></param>
        /// <param name="useLastBom"></param>
        /// <param name="def"></param>
        /// <returns></returns>
        public static IObservable<T[]> ParsePackage<T>(
            this IObservable<T> source,
            T bom,
            T eom,
            bool useLastBom = false,
            T[] def = null
        )
            where T : IEquatable<T>
        {
            if (bom == null || eom == null)
                throw new ArgumentException("bom or eom is null.");

            if (bom.Equals(eom))
                throw new ArgumentException("bom = eom is not allowed.");

            return source.ParsePackage(new[] { bom }, new[] { eom }, useLastBom, def);
        }

        /// <summary>
        ///     包头包尾分包
        /// </summary>
        /// <param name="source"></param>
        /// <param name="bom"></param>
        /// <param name="eom"></param>
        /// <param name="useLastBom"></param>
        /// <param name="def"></param>
        /// <returns></returns>
        public static IObservable<T[]> ParsePackage<T>(
            this IObservable<T> source,
            T[] bom,
            T[] eom,
            bool useLastBom = false,
            T[] def = null
        )
            where T : IEquatable<T>
        {
            if (bom == null || eom == null || bom.Length == 0 || eom.Length == 0)
                throw new ArgumentException("bom or eom is empty.");

            if (bom.SequenceEqual(eom))
                throw new ArgumentException("bom = eom is not allowed.");

            return Observable.Create<T[]>(observer =>
            {
                var queue = new Queue<T>();

                var disposable = source.Subscribe(b =>
                    {
                        queue.Enqueue(b);

                        var queueCount = queue.Count;
                        var bomLength = bom.Length;

                        if (queueCount == bomLength && !queue.SequenceEqual(bom))
                        {
                            queue.Dequeue();
                            return;
                        }

                        if (useLastBom && queueCount > bomLength &&
                            queue.Skip(queueCount - bomLength).SequenceEqual(bom))
                        {
                            for (var i = 0; i < queueCount - bomLength; i++)
                                queue.Dequeue();

                            return;
                        }

                        if (queueCount <= bom.Length + eom.Length)
                            return;

                        if (queue.Take(bomLength).SequenceEqual(bom) &&
                            queue.Skip(queueCount - eom.Length).SequenceEqual(eom))
                        {
                            observer.OnNext(queue.ToArray());

                            queue.Clear();
                        }
                    },
                    observer.OnError,
                    observer.OnCompleted);

                return disposable;
            });
        }

        /// <summary>
        ///     包头包尾分包
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="bom"></param>
        /// <param name="eom"></param>
        /// <param name="useLastBom"></param>
        /// <returns></returns>
        public static IObservable<T[]> ParsePackage<T>(
            this IObservable<IEnumerable<T>> source,
            T[] bom,
            T[] eom,
            bool useLastBom = false
        )
        {
            if (bom == null || eom == null || bom.Length == 0 || eom.Length == 0)
                throw new ArgumentException("bom or eom is empty.");

            if (bom.SequenceEqual(eom))
                throw new ArgumentException("bom = eom is not allowed.");

            return Observable.Create<T[]>(observer =>
            {
                var queue = new Queue<T>();

                var disposable = source.Subscribe(bytes =>
                    {
                        foreach (var b in bytes)
                        {
                            queue.Enqueue(b);

                            var queueCount = queue.Count;
                            var bomLength = bom.Length;

                            if (queueCount == bomLength && !queue.SequenceEqual(bom))
                            {
                                queue.Dequeue();
                                continue;
                            }

                            if (useLastBom && queueCount > bomLength &&
                                queue.Skip(queueCount - bomLength).SequenceEqual(bom))
                            {
                                for (var i = 0; i < queueCount - bomLength; i++)
                                    queue.Dequeue();

                                continue;
                            }

                            if (queueCount <= bom.Length + eom.Length)
                                continue;

                            if (queue.Take(bomLength).SequenceEqual(bom) &&
                                queue.Skip(queueCount - eom.Length).SequenceEqual(eom))
                            {
                                observer.OnNext(queue.ToArray());

                                queue.Clear();
                            }
                        }
                    },
                    observer.OnError,
                    observer.OnCompleted);

                return disposable;
            });
        }

        #endregion

        #region 包头包尾+超时分包

        /// <summary>
        ///     包头包尾+超时分包
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="bom"></param>
        /// <param name="eom"></param>
        /// <param name="timeoutMs">100ms+</param>
        /// <param name="useLastBom"></param>
        /// <param name="def"></param>
        /// <returns></returns>
        public static IObservable<T[]> ParsePackage<T>(
            this IObservable<T> source,
            T bom,
            T eom,
            int timeoutMs,
            bool useLastBom = false,
            T[] def = null
        )
            where T : IEquatable<T>
        {
            if (bom == null || eom == null)
                throw new ArgumentException("bom or eom is null.");

            return source.ParsePackage(new[] { bom }, new[] { eom }, timeoutMs, useLastBom, def);
        }

        /// <summary>
        ///     包头包尾+超时分包
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="bom"></param>
        /// <param name="eom"></param>
        /// <param name="timeoutMs">100ms+</param>
        /// <param name="useLastBom"></param>
        /// <param name="def"></param>
        /// <returns></returns>
        public static IObservable<T[]> ParsePackage<T>(
            this IObservable<T> source,
            T[] bom,
            T[] eom,
            int timeoutMs,
            bool useLastBom = false,
            T[] def = null
        )
            where T : IEquatable<T>
        {
            if (bom == null || eom == null || bom.Length == 0 || eom.Length == 0)
                throw new ArgumentException("bom or eom is empty.");

            if (bom.SequenceEqual(eom))
                throw new ArgumentException("bom = eom is not allowed.");

            return Observable.Using(
                () => new ParsePackageBeginEndTimeoutState<T>(bom, eom, timeoutMs, useLastBom),
                r =>
                    source
                        .Scan(
                            r,
                            (acc, v) =>
                            {
                                acc.Add(v);
                                return acc;
                            }
                        )
                        .Select(p => p.Get() ?? def)
                        .Where(p => p != null)
            );
        }

        #endregion

        #region 包头+固定长度(含包头)分包

        /// <summary>
        ///     包头+固定长度(含包头)分包
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="bom"></param>
        /// <param name="count"></param>
        /// <param name="useLastBom"></param>
        /// <returns></returns>
        public static IObservable<T[]> ParsePackage<T>(
            this IObservable<T> source,
            T bom,
            int count,
            bool useLastBom = false
        )
            where T : IEquatable<T>
        {
            if (bom == null)
                throw new ArgumentException("bom is null.");
            if (count <= 1)
                throw new ArgumentException("count <= 1");

            return source.ParsePackage(new[] { bom }, count, useLastBom);
        }

        /// <summary>
        ///     包头+固定长度(含包头)分包
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="bom"></param>
        /// <param name="count"></param>
        /// <param name="useLastBom"></param>
        /// <param name="def"></param>
        /// <returns></returns>
        public static IObservable<T[]> ParsePackage<T>(
            this IObservable<T> source,
            T[] bom,
            int count,
            bool useLastBom = false,
            T[] def = null
        )
            where T : IEquatable<T>
        {
            if (bom == null || bom.Length == 0)
                throw new ArgumentException("bom is empty.");

            if (count <= bom.Length)
                throw new ArgumentException("count <= bom.Length");

            return Observable.Create<T[]>(observer =>
            {
                var queue = new Queue<T>();

                var disposable = source.Subscribe(b =>
                    {
                        queue.Enqueue(b);

                        var bomLength = bom.Length;
                        var queueCount = queue.Count;

                        if (queueCount == bomLength && !queue.SequenceEqual(bom))
                        {
                            queue.Dequeue();
                            return;
                        }

                        if (useLastBom && queueCount > bomLength &&
                            queue.Skip(queueCount - bomLength).SequenceEqual(bom))
                        {
                            for (var i = 0; i < queueCount - bomLength; i++)
                                queue.Dequeue();

                            return;
                        }

                        if (queueCount < count)
                            return;

                        observer.OnNext(queue.ToArray());
                        queue.Clear();
                    },
                    observer.OnError,
                    observer.OnCompleted);

                return disposable;
            });
        }

        /// <summary>
        ///     包头+固定长度(含包头)分包
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="bom"></param>
        /// <param name="count"></param>
        /// <param name="useLastBom"></param>
        /// <returns></returns>
        public static IObservable<T[]> ParsePackage<T>(
            this IObservable<IEnumerable<T>> source,
            T[] bom,
            int count,
            bool useLastBom = false
        )
            where T : IEquatable<T>
        {
            if (bom == null || bom.Length == 0)
                throw new ArgumentException("bom is empty.");

            if (count <= bom.Length)
                throw new ArgumentException("count <= bom.Length");

            return Observable.Create<T[]>(observer =>
            {
                var queue = new Queue<T>();

                var disposable = source.Subscribe(bytes =>
                    {
                        foreach (var b in bytes)
                        {
                            queue.Enqueue(b);

                            var bomLength = bom.Length;
                            var queueCount = queue.Count;

                            if (queueCount == bomLength && !queue.SequenceEqual(bom))
                            {
                                queue.Dequeue();
                                continue;
                            }

                            if (useLastBom && queueCount > bomLength &&
                                queue.Skip(queueCount - bomLength).SequenceEqual(bom))
                            {
                                for (var i = 0; i < queueCount - bomLength; i++)
                                    queue.Dequeue();

                                continue;
                            }

                            if (queueCount < count)
                                continue;

                            observer.OnNext(queue.ToArray());
                            queue.Clear();
                        }
                    },
                    observer.OnError,
                    observer.OnCompleted);

                return disposable;
            });
        }

        #endregion

        #region 包头+固定长度(含包头)+超时分包

        /// <summary>
        ///     包头+固定长度(含包头)+超时分包
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="bom"></param>
        /// <param name="count"></param>
        /// <param name="timeoutMs">100ms+</param>
        /// <param name="useLastBom"></param>
        /// <param name="def"></param>
        /// <returns></returns>
        public static IObservable<T[]> ParsePackage<T>(
            this IObservable<T> source,
            T bom,
            int count,
            int timeoutMs,
            bool useLastBom = false,
            T[] def = null
        )
            where T : IEquatable<T>
        {
            if (bom == null)
                throw new ArgumentException("bom is null.");

            if (count <= 1)
                throw new ArgumentException("count <= 1");

            return source.ParsePackage(new[] { bom }, count, timeoutMs, useLastBom, def);
        }

        /// <summary>
        ///     包头+固定长度(含包头)+超时分包
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="bom"></param>
        /// <param name="count"></param>
        /// <param name="timeoutMs">100ms+</param>
        /// <param name="useLastBom"></param>
        /// <param name="def"></param>
        /// <returns></returns>
        public static IObservable<T[]> ParsePackage<T>(
            this IObservable<T> source,
            T[] bom,
            int count,
            int timeoutMs,
            bool useLastBom = false,
            T[] def = null
        )
            where T : IEquatable<T>
        {
            if (bom.Length == 0)
                throw new ArgumentException("bom is empty.");

            if (count <= bom.Length)
                throw new ArgumentException("count <= bom.Length");

            return Observable.Using(
                () => new ParsePackageBeginCountTimeoutState<T>(bom, count, timeoutMs, useLastBom),
                r =>
                    source
                        .Scan(
                            r,
                            (acc, v) =>
                            {
                                acc.Add(v);
                                return acc;
                            }
                        )
                        .Select(p => p.Get() ?? def)
                        .Where(p => p != null)
            );
        }

        #endregion

        #region Helpers

        private sealed class ParsePackageBeginEndTimeoutState<T> : IDisposable
            where T : IEquatable<T>
        {
            private readonly T[] _bom;
            private readonly SerialDisposable _dis = new SerialDisposable();
            private readonly T[] _eom;
            private readonly int _timeoutMs;
            private readonly bool _useLastBom;

            // ReSharper disable once FieldCanBeMadeReadOnly.Local
            private ConcurrentQueue<T> _queue = new ConcurrentQueue<T>();

            public ParsePackageBeginEndTimeoutState(
                T[] bom,
                T[] eom,
                int timeoutMs,
                bool useLastBom
            )
            {
                _bom = bom;
                _eom = eom;
                _timeoutMs = timeoutMs;
                _useLastBom = useLastBom;
            }

            public void Dispose()
            {
                _dis.Dispose();
            }

            private void CreateTimer()
            {
                _dis.Disposable = Observable
                    .Interval(TimeSpan.FromMilliseconds(_timeoutMs))
                    .Subscribe(_ =>
                    {
                        if (Get() == null)
#if NETSTANDARD2_1
                            _queue.Clear();
#else
                            _queue = new ConcurrentQueue<T>();
#endif
                    });
            }

            public T[] Get()
            {
                var arr = _queue.ToArray();
                if (
                    arr.Take(_bom.Length).SequenceEqual(_bom)
                    && arr.Skip(arr.Length - _eom.Length).SequenceEqual(_eom)
                )
                {
#if NETSTANDARD2_1
                    _queue.Clear();
#else
                    _queue = new ConcurrentQueue<T>();
#endif
                    return arr;
                }

                return null;
            }

            public void Add(T obj)
            {
                CreateTimer();

                _queue.Enqueue(obj);

                var count = _queue.Count;
                var length = _bom.Length;

                if (count == length && !_queue.SequenceEqual(_bom))
                    _queue.TryDequeue(out _);

                if (_useLastBom && count > length)
                    if (_queue.Skip(count - length).Take(length).SequenceEqual(_bom))
                        for (var i = 0; i < count - length; i++)
                            _queue.TryDequeue(out _);
            }
        }

        private sealed class ParsePackageBeginCountTimeoutState<T> : IDisposable
            where T : IEquatable<T>
        {
            private readonly T[] _bom;
            private readonly int _count;
            private readonly SerialDisposable _dis = new SerialDisposable();
            private readonly int _timeoutMs;
            private readonly bool _useLastBom;

            // ReSharper disable once FieldCanBeMadeReadOnly.Local
            private ConcurrentQueue<T> _queue = new ConcurrentQueue<T>();

            public ParsePackageBeginCountTimeoutState(
                T[] bom,
                int count,
                int timeoutMs,
                bool useLastBom
            )
            {
                _bom = bom;
                _count = count;
                _timeoutMs = timeoutMs;
                _useLastBom = useLastBom;
            }

            public void Dispose()
            {
                _dis.Dispose();
            }

            private void CreateTimer()
            {
                _dis.Disposable = Observable
                    .Interval(TimeSpan.FromMilliseconds(_timeoutMs))
                    .Subscribe(_ =>
                    {
                        if (Get() == null)
#if NETSTANDARD2_1
                            _queue.Clear();
#else
                            _queue = new ConcurrentQueue<T>();
#endif
                    });
            }

            public T[] Get()
            {
                var arr = _queue.ToArray();
                if (arr.Take(_bom.Length).SequenceEqual(_bom) && arr.Count() >= _count)
                {
#if NETSTANDARD2_1
                    _queue.Clear();
#else
                    _queue = new ConcurrentQueue<T>();
#endif
                    return arr.Take(_count).ToArray();
                }

                return null;
            }

            public void Add(T obj)
            {
                CreateTimer();

                _queue.Enqueue(obj);

                var count = _queue.Count;
                var length = _bom.Length;

                if (count == length && !_queue.SequenceEqual(_bom))
                    _queue.TryDequeue(out _);

                if (_useLastBom && count > length)
                    if (_queue.Skip(count - length).Take(length).SequenceEqual(_bom))
                        for (var i = 0; i < count - length; i++)
                            _queue.TryDequeue(out _);
            }
        }

        #endregion
    }
}