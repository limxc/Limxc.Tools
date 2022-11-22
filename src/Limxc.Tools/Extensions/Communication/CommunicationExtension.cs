using System;
using System.Collections.Concurrent;
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
        ///     1包头1包尾分包
        /// </summary>
        /// <param name="source"></param>
        /// <param name="bom"></param>
        /// <param name="eom"></param>
        /// <returns></returns>
        public static IObservable<T[]> ParsePackage<T>(this IObservable<T> source, T bom, T eom) where T : IEquatable<T>
        {
            return source.Publish(s =>
            {
                return Observable.Defer
                (() => s
                    .SkipWhile(b => !b.Equals(bom))
                    .Skip(1)
                    .TakeWhile(b => !b.Equals(eom))
                    .ToArray()
                    .Where(p => p.Length > 0)
                    .Repeat()
                );
            });
        }

        /// <summary>
        ///     包头包尾分包
        /// </summary>
        /// <param name="source"></param>
        /// <param name="bom"></param>
        /// <param name="eom"></param>
        /// <returns></returns>
        public static IObservable<T[]> ParsePackage<T>(this IObservable<T> source, T[] bom, T[] eom)
            where T : IEquatable<T>
        {
            if (bom.Length == 0 || eom.Length == 0)
                throw new ArgumentException("If bom or eom is empty, use SeparatorMessagePackParser.");

            return Observable.Create<T[]>(o =>
            {
                var rst = new ConcurrentQueue<T>();
                var bQueue = new ConcurrentQueue<T>();
                var eQueue = new ConcurrentQueue<T>();
                var bl = bom.Length;
                var el = eom.Length;
                var bomFound = false;

                var dis = source.Subscribe(s =>
                {
                    if (!bomFound)
                    {
                        bQueue.Enqueue(s);
                        if (bQueue.Count == bl)
                        {
                            if (bQueue.AsEnumerable().SequenceEqual(bom))
                                bomFound = true;
                            else
                                bQueue.TryDequeue(out _);
                        }
                    }
                    else
                    {
                        rst.Enqueue(s);
                        eQueue.Enqueue(s);
                        if (eQueue.Count == el)
                        {
                            if (eQueue.AsEnumerable().SequenceEqual(eom))
                            {
#if NETSTANDARD2_1
                                o.OnNext(rst.SkipLast(el).ToArray());
                                rst.Clear();
                                bQueue.Clear();
                                eQueue.Clear();
#else
                                o.OnNext(rst.Take(rst.Count - el).ToArray());
                                while (rst.TryDequeue(out _))
                                {
                                }

                                while (bQueue.TryDequeue(out _))
                                {
                                }

                                while (eQueue.TryDequeue(out _))
                                {
                                }
#endif
                                bomFound = false;
                            }
                            else
                            {
                                eQueue.TryDequeue(out _);
                            }
                        }
                    }
                }, o.OnCompleted);

                return Disposable.Create(() =>
                {
                    o.OnCompleted();
                    dis.Dispose();
                    rst = null;
                    bQueue = null;
                    eQueue = null;
                });
            });
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
            return Observable.Create<T[]>(o =>
            {
                var rst = new ConcurrentQueue<T>();
                var tmp = new ConcurrentQueue<T>();
                var len = separator.Length;

                var dis = source.Subscribe(s =>
                {
                    rst.Enqueue(s);
                    tmp.Enqueue(s);
                    if (tmp.Count == len)
                    {
                        if (tmp.AsEnumerable().SequenceEqual(separator))
                        {
#if NETSTANDARD2_1
                            o.OnNext(rst.SkipLast(len).ToArray());
                            rst.Clear();
                            tmp.Clear();
#else
                            o.OnNext(rst.Take(rst.Count - len).ToArray());
                            while (rst.TryDequeue(out _))
                            {
                            }

                            while (tmp.TryDequeue(out _))
                            {
                            }
#endif
                        }
                        else
                        {
                            tmp.TryDequeue(out _);
                        }
                    }
                }, o.OnCompleted);

                return Disposable.Create(() =>
                {
                    o.OnCompleted();
                    dis.Dispose();
                    rst = null;
                    tmp = null;
                });
            }).Where(p => p.Length > 0);
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
                    .Subscribe(p =>
                    {
                        //timer.Stop();
                        rst.Enqueue(p);
                        //timer.Start();
                    }).DisposeWith(dis);

                return dis;
            });
        }

        #endregion 分包处理
    }
}