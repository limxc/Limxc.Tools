﻿using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace Limxc.Tools.Extensions.Communication
{
    public static class ParseExtension
    {
        #region 分包处理(数组输入)

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
            return Observable.Create<T[]>(observer =>
            {
                var queue = new Queue<T>();
                var disposable = source.Subscribe(bytes =>
                {
                    lock (((ICollection)queue).SyncRoot)
                    {
                        foreach (var b in bytes)
                        {
                            queue.Enqueue(b);

                            if (queue.Count <= separator.Length)
                                continue;

                            if (queue.Skip(queue.Count - separator.Length).SequenceEqual(separator))
                            {
                                observer.OnNext(queue.ToArray());

                                queue.Clear();
                            }
                        }
                    }
                });

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
            return Observable.Create<T[]>(observer =>
            {
                var queue = new Queue<T>();

                var disposable = source.Subscribe(bytes =>
                {
                    lock (((ICollection)queue).SyncRoot)
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
                    }
                });

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
            return Observable.Create<T[]>(observer =>
            {
                var queue = new Queue<T>();

                var disposable = source.Subscribe(bytes =>
                {
                    lock (((ICollection)queue).SyncRoot)
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
                    }
                });

                return disposable;
            });
        }

        /// <summary>
        ///     一定时间后没有新数据则分包
        ///     (精度不高,不适合时间间隔过小的情况)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="timeSpan"></param>
        /// <returns></returns>
        public static IObservable<T[]> ParsePackage<T>(this IObservable<T[]> source, TimeSpan timeSpan)
        {
            return source
                .Buffer(source.Throttle(timeSpan))
                .Where(p => p != null && p.Count > 0)
                .Select(p =>
                {
                    var arr = new T[p.Select(v => v.Length).Sum()];
                    var len = 0;
                    foreach (var t in p)
                    {
                        t.CopyTo(arr, len);
                        len += t.Length;
                    }

                    return arr;
                });
        }

        #endregion

        #region 分包处理

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
            if (separator.Length == 0)
                throw new ArgumentException("separator is empty.");

            return source
                .Scan(
                    new ParsePackageSeparatorState<T>(separator),
                    (acc, v) =>
                    {
                        acc.Add(v);
                        return acc;
                    }
                )
                .Select(p => p.Get())
                .Where(p => p != null && p.Length > separator.Length);
        }

        /// <summary>
        ///     模式分包, 输出两pattern之间的数组
        /// </summary>
        /// <param name="source"></param>
        /// <param name="pattern"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static IObservable<byte[]> ParsePackage(
            this IObservable<byte[]> source,
            byte[] pattern
        )
        {
            if (pattern.Length == 0)
                throw new ArgumentException("pattern is empty.");

            return source
                .Scan(
                    new ParsePackageBytePatternState(pattern),
                    (acc, v) =>
                    {
                        acc.Add(v);
                        return acc;
                    }
                )
                .Select(p => p.Get())
                .SelectMany(p => p)
                .Where(p => p != null && p.Length > pattern.Length);
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
            T bom,
            T eom,
            bool useLastBom = false,
            T[] def = null
        )
            where T : IEquatable<T>
        {
            if (bom == null || eom == null)
                throw new ArgumentException("bom or eom is null.");

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
            if (bom.SequenceEqual(eom))
                throw new ArgumentException("bom = eom is not allowed.");
            if (bom.Length == 0 || eom.Length == 0)
                throw new ArgumentException("bom or eom is empty.");

            return source
                .Scan(
                    new ParsePackageBeginEndState<T>(bom, eom, useLastBom),
                    (acc, v) =>
                    {
                        acc.Add(v);
                        return acc;
                    }
                )
                .Select(p => p.Get() ?? def)
                .Where(p => p != null);
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
            this IObservable<T> source,
            T bom,
            int count,
            bool useLastBom = false
        )
            where T : IEquatable<T>
        {
            if (bom == null)
                throw new ArgumentException("bom is null.");

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
            if (bom.Length == 0)
                throw new ArgumentException("bom is empty.");

            return source
                .Scan(
                    new ParsePackageBeginCountState<T>(bom, count, useLastBom),
                    (acc, v) =>
                    {
                        acc.Add(v);
                        return acc;
                    }
                )
                .Select(p => p.Get() ?? def)
                .Where(p => p != null);
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
            if (bom.SequenceEqual(eom))
                throw new ArgumentException("bom = eom is not allowed.");
            if (bom.Length == 0 || eom.Length == 0)
                throw new ArgumentException("bom or eom is empty.");

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

        /// <summary>
        ///     一定时间后没有新数据则分包
        ///     (精度不高,不适合时间间隔过小的情况)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="timeSpan"></param>
        /// <returns></returns>
        public static IObservable<T[]> ParsePackage<T>(this IObservable<T> source, TimeSpan timeSpan)
        {
            return source
                .Buffer(source.Throttle(timeSpan))
                .Where(p => p != null && p.Count > 0)
                .Select(p => p.ToArray());
        }

        #endregion 分包处理

        #region Helpers

        private sealed class ParsePackageSeparatorState<T>
            where T : IEquatable<T>
        {
            private readonly Queue<T> _queue = new Queue<T>();
            private readonly T[] _separator;

            public ParsePackageSeparatorState(T[] separator)
            {
                _separator = separator;
            }

            public T[] Get()
            {
                var arr = _queue.ToArray();

                if (_queue.Skip(arr.Length - _separator.Length).SequenceEqual(_separator))
                {
                    _queue.Clear();
                    return arr;
                }

                return null;
            }

            public void Add(T obj)
            {
                _queue.Enqueue(obj);
            }
        }

        /// <summary>
        ///     高性能byte分包
        /// </summary>
        private sealed class ParsePackageBytePatternState
        {
            private readonly byte[] _separator;
            private IEnumerable<byte> _remain = Array.Empty<byte>();

            public ParsePackageBytePatternState(byte[] separator)
            {
                _separator = separator;
            }

            public byte[][] Get()
            {
                var r = _remain.ToArray().LocateToPack(_separator);
                _remain = r.Remain;

                return r.Pack;
            }

            public void Add(params byte[] obj)
            {
                _remain = _remain.Concat(obj);
            }
        }

        private sealed class ParsePackageBeginEndState<T>
            where T : IEquatable<T>
        {
            private readonly T[] _bom;
            private readonly T[] _eom;
            private readonly Queue<T> _queue = new Queue<T>();
            private readonly bool _useLastBom;

            public ParsePackageBeginEndState(T[] bom, T[] eom, bool useLastBom)
            {
                _bom = bom;
                _eom = eom;
                _useLastBom = useLastBom;
            }

            public T[] Get()
            {
                var arr = _queue.ToArray();
                if (
                    arr.Take(_bom.Length).SequenceEqual(_bom)
                    && arr.Skip(arr.Length - _eom.Length).SequenceEqual(_eom)
                )
                {
                    _queue.Clear();
                    return arr;
                }

                return null;
            }

            public void Add(T obj)
            {
                _queue.Enqueue(obj);

                var count = _queue.Count;
                var length = _bom.Length;

                if (count == length && !_queue.SequenceEqual(_bom))
                    _queue.Dequeue();

                if (_useLastBom && count > length)
                    if (_queue.Skip(count - length).Take(length).SequenceEqual(_bom))
                        for (var i = 0; i < count - length; i++)
                            _queue.Dequeue();
            }
        }

        private sealed class ParsePackageBeginCountState<T>
            where T : IEquatable<T>
        {
            private readonly T[] _bom;
            private readonly int _count;
            private readonly Queue<T> _queue = new Queue<T>();
            private readonly bool _useLastBom;

            public ParsePackageBeginCountState(T[] bom, int count, bool useLastBom)
            {
                _bom = bom;
                _count = count;
                _useLastBom = useLastBom;
            }

            public T[] Get()
            {
                var arr = _queue.ToArray();
                if (arr.Take(_bom.Length).SequenceEqual(_bom) && arr.Count() >= _count)
                {
                    _queue.Clear();
                    return arr.Take(_count).ToArray();
                }

                return null;
            }

            public void Add(T obj)
            {
                _queue.Enqueue(obj);

                var count = _queue.Count;
                var length = _bom.Length;

                if (count == length && !_queue.SequenceEqual(_bom))
                    _queue.Dequeue();

                if (_useLastBom && count > length)
                    if (_queue.Skip(count - length).Take(length).SequenceEqual(_bom))
                        for (var i = 0; i < count - length; i++)
                            _queue.Dequeue();
            }
        }

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