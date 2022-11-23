using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Limxc.Tools.Bases.Communication;

namespace Limxc.Tools.Extensions.Communication
{
    public static class CommunicationExtension
    {
        /// <summary>
        ///     返回值匹配及解析
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="resp"></param>
        /// <param name="byteToStringConverter">默认:<see cref="DataConversionExtension.IntToHex" /></param>
        /// <returns></returns>
        public static IObservable<CommContext> FindResponse(this IObservable<CommContext> cmd, IObservable<byte[]> resp,
            Func<byte[], string> byteToStringConverter = null)
        {
            if (byteToStringConverter == null)
                byteToStringConverter = DataConversionExtension.ByteToHex;

            return cmd
                    .Where(p => p.SendTime != null)
                    .SelectMany(p =>
                    {
                        if (p.Timeout == 0 || string.IsNullOrWhiteSpace(p.Response.Template))
                        {
                            p.State = CommContextState.NoNeed;
                            return Observable.Return(p);
                        }

                        Debug.Assert(p.SendTime != null, "CommContext.SendTime != null");
                        var st = ((DateTime)p.SendTime).ToDateTimeOffset();

                        return resp.Timestamp()
                                .Select(d => new Timestamped<string>(byteToStringConverter(d.Value), d.Timestamp))
                                .SkipUntil(st)
                                .TakeUntil(st.AddMilliseconds(p.Timeout))
                                .Select(r => r.Value)
                                .Scan((acc, r) => acc + r)
                                .FirstOrDefaultAsync(t => p.Response.Template.IsTemplateMatch(t, '$', false))
                                .Select(r =>
                                {
                                    if (r != null)
                                    {
                                        p.Response.Value = p.Response.Template.TryGetTemplateMatchResult(r);
                                        p.ReceivedTime = DateTime.Now;
                                        p.State = CommContextState.Success;
                                    }
                                    else
                                    {
                                        p.State = CommContextState.Timeout;
                                    }

                                    return p;
                                })
                                .Retry()
                            ;
                    })
                ;
        }

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

            return
                source.ParsePackage(new[] { separator });
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

            return
                source
                    .Scan(new ParsePackageSeparatorState<T>(separator),
                        (acc, v) =>
                        {
                            acc.Add(v);
                            return acc;
                        })
                    .Select(p => p.Get())
                    .Where(p => p != null && p.Length > separator.Length);
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
        public static IObservable<T[]> ParsePackage<T>(this IObservable<T> source, T bom, T eom,
            bool useLastBom = false, T[] def = null) where T : IEquatable<T>
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
        public static IObservable<T[]> ParsePackage<T>(this IObservable<T> source, T[] bom, T[] eom,
            bool useLastBom = false, T[] def = null)
            where T : IEquatable<T>
        {
            if (bom.SequenceEqual(eom))
                throw new ArgumentException("bom = eom is not allowed.");
            if (bom.Length == 0 || eom.Length == 0)
                throw new ArgumentException("bom or eom is empty.");

            return
                source
                    .Scan(new ParsePackageBeginEndState<T>(bom, eom, useLastBom),
                        (acc, v) =>
                        {
                            acc.Add(v);
                            return acc;
                        })
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
        public static IObservable<T[]> ParsePackage<T>(this IObservable<T> source, T bom, int count,
            bool useLastBom = false)
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
        public static IObservable<T[]> ParsePackage<T>(this IObservable<T> source, T[] bom, int count,
            bool useLastBom = false,
            T[] def = null)
            where T : IEquatable<T>
        {
            if (bom.Length == 0)
                throw new ArgumentException("bom is empty.");

            return source
                .Scan(new ParsePackageBeginCountState<T>(bom, count, useLastBom),
                    (acc, v) =>
                    {
                        acc.Add(v);
                        return acc;
                    })
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
        public static IObservable<T[]> ParsePackage<T>(this IObservable<T> source, T bom, T eom, int timeoutMs,
            bool useLastBom = false,
            T[] def = null)
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
        public static IObservable<T[]> ParsePackage<T>(this IObservable<T> source, T[] bom, T[] eom, int timeoutMs,
            bool useLastBom = false,
            T[] def = null)
            where T : IEquatable<T>
        {
            if (bom.SequenceEqual(eom))
                throw new ArgumentException("bom = eom is not allowed.");
            if (bom.Length == 0 || eom.Length == 0)
                throw new ArgumentException("bom or eom is empty.");

            return Observable.Using(() => new ParsePackageBeginEndTimeoutState<T>(bom, eom, timeoutMs, useLastBom), r =>
                source
                    .Scan(r,
                        (acc, v) =>
                        {
                            acc.Add(v);
                            return acc;
                        })
                    .Select(p => p.Get() ?? def)
                    .Where(p => p != null));
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
        public static IObservable<T[]> ParsePackage<T>(this IObservable<T> source, T bom, int count, int timeoutMs,
            bool useLastBom = false,
            T[] def = null)
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
        public static IObservable<T[]> ParsePackage<T>(this IObservable<T> source, T[] bom, int count, int timeoutMs,
            bool useLastBom = false,
            T[] def = null)
            where T : IEquatable<T>
        {
            if (bom.Length == 0)
                throw new ArgumentException("bom is empty.");

            return Observable.Using(() => new ParsePackageBeginCountTimeoutState<T>(bom, count, timeoutMs, useLastBom),
                r => source
                    .Scan(r,
                        (acc, v) =>
                        {
                            acc.Add(v);
                            return acc;
                        })
                    .Select(p => p.Get() ?? def)
                    .Where(p => p != null));
        }

        /// <summary>
        ///     一定时间后没有新数据则分包
        ///     (精度不高,不适合时间间隔过小的情况)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obs"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        public static IObservable<T[]> ParsePackage<T>(this IObservable<T> obs, TimeSpan time)
        {
            return Observable.Create<T[]>(o =>
            {
                var dis = new CompositeDisposable();
                var rst = new ConcurrentQueue<T>();

                obs.Throttle(time)
                    .Subscribe(p =>
                    {
                        var arr = rst.ToArray();
                        if (arr.Any())
                        {
                            o.OnNext(rst.ToArray());
#if NETSTANDARD2_1
                            rst.Clear();
#else
                            rst = new ConcurrentQueue<T>();
#endif
                        }
                    }).DisposeWith(dis);

                obs
                    .Subscribe(p => { rst.Enqueue(p); }).DisposeWith(dis);

                return dis;
            });
        }

        #endregion 分包处理

        #region Helpers

        private sealed class ParsePackageSeparatorState<T> where T : IEquatable<T>
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

                var count = _queue.Count;
                var length = _separator.Length;
                if (count >= length)
                    if (_queue.Skip(count - length).Take(length).SequenceEqual(_separator))

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

        private sealed class ParsePackageBeginEndState<T> where T : IEquatable<T>
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
                if (arr.Take(_bom.Length).SequenceEqual(_bom) &&
                    arr.Skip(arr.Length - _eom.Length).SequenceEqual(_eom))
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

                if (count == length && !_queue.SequenceEqual(_bom)) _queue.Dequeue();

                if (_useLastBom && count > length)
                    if (_queue.Skip(count - length).Take(length).SequenceEqual(_bom))
                        for (var i = 0; i < count - length; i++)
                            _queue.Dequeue();
            }
        }

        private sealed class ParsePackageBeginCountState<T> where T : IEquatable<T>
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
                if (arr.Take(_bom.Length).SequenceEqual(_bom) &&
                    arr.Count() >= _count)
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

                if (count == length && !_queue.SequenceEqual(_bom)) _queue.Dequeue();

                if (_useLastBom && count > length)
                    if (_queue.Skip(count - length).Take(length).SequenceEqual(_bom))
                        for (var i = 0; i < count - length; i++)
                            _queue.Dequeue();
            }
        }

        private sealed class ParsePackageBeginEndTimeoutState<T> : IDisposable where T : IEquatable<T>
        {
            private readonly T[] _bom;
            private readonly SerialDisposable _dis = new SerialDisposable();
            private readonly T[] _eom;
            private readonly Queue<T> _queue = new Queue<T>();
            private readonly int _timeoutMs;
            private readonly bool _useLastBom;

            public ParsePackageBeginEndTimeoutState(T[] bom, T[] eom, int timeoutMs, bool useLastBom)
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
                _dis.Disposable = Observable.Interval(TimeSpan.FromMilliseconds(_timeoutMs))
                    .Subscribe(_ =>
                    {
                        if (Get() == null)
                            _queue.Clear();
                    });
            }

            public T[] Get()
            {
                var arr = _queue.ToArray();
                if (arr.Take(_bom.Length).SequenceEqual(_bom) &&
                    arr.Skip(arr.Length - _eom.Length).SequenceEqual(_eom))
                {
                    _queue.Clear();
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

                if (count == length && !_queue.SequenceEqual(_bom)) _queue.Dequeue();

                if (_useLastBom && count > length)
                    if (_queue.Skip(count - length).Take(length).SequenceEqual(_bom))
                        for (var i = 0; i < count - length; i++)
                            _queue.Dequeue();
            }
        }

        private sealed class ParsePackageBeginCountTimeoutState<T> : IDisposable where T : IEquatable<T>
        {
            private readonly T[] _bom;
            private readonly int _count;
            private readonly SerialDisposable _dis = new SerialDisposable();
            private readonly Queue<T> _queue = new Queue<T>();
            private readonly int _timeoutMs;
            private readonly bool _useLastBom;

            public ParsePackageBeginCountTimeoutState(T[] bom, int count, int timeoutMs, bool useLastBom)
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
                _dis.Disposable = Observable.Interval(TimeSpan.FromMilliseconds(_timeoutMs))
                    .Subscribe(_ =>
                    {
                        if (Get() == null)
                            _queue.Clear();
                    });
            }

            public T[] Get()
            {
                var arr = _queue.ToArray();
                if (arr.Take(_bom.Length).SequenceEqual(_bom) &&
                    arr.Count() >= _count)
                {
                    _queue.Clear();
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

                if (count == length && !_queue.SequenceEqual(_bom)) _queue.Dequeue();

                if (_useLastBom && count > length)
                    if (_queue.Skip(count - length).Take(length).SequenceEqual(_bom))
                        for (var i = 0; i < count - length; i++)
                            _queue.Dequeue();
            }
        }

        #endregion
    }
}