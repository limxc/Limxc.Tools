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

            var list = new List<T>();
            var bQueue = new ConcurrentQueue<T>();
            var eQueue = new ConcurrentQueue<T>();
            var bl = bom.Length;
            var el = eom.Length;
            var findHeader = false;

            return Observable.Create<T[]>(o =>
            {
                var dis = source.Subscribe(s =>
                {
                    if (!findHeader)
                    {
                        bQueue.Enqueue(s);
                        if (bQueue.Count == bl)
                        {
                            if (bQueue.AsEnumerable().SequenceEqual(bom))
                                findHeader = true;
                            else
                                bQueue.TryDequeue(out _);
                        }
                    }
                    else
                    {
                        list.Add(s);
                        eQueue.Enqueue(s);
                        if (eQueue.Count == el)
                        {
                            if (eQueue.AsEnumerable().SequenceEqual(eom))
                            {
#if NETSTANDARD2_1
                                o.OnNext(list.SkipLast(el).ToArray());
                                bQueue.Clear();
                                eQueue.Clear();
#else
                                o.OnNext(list.Take(list.Count - el).ToArray());
                                while (!bQueue.IsEmpty) bQueue.TryDequeue(out _);
                                while (!eQueue.IsEmpty) eQueue.TryDequeue(out _);
#endif
                                list.Clear();
                                findHeader = false;
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
                    list.Clear();
                    list = null;
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
            var list = new List<T>();
            var queue = new ConcurrentQueue<T>();
            var len = separator.Length;

            return Observable.Create<T[]>(o =>
            {
                var dis = source.Subscribe(s =>
                {
                    list.Add(s);
                    queue.Enqueue(s);
                    if (queue.Count == len)
                    {
                        if (queue.AsEnumerable().SequenceEqual(separator))
                        {
#if NETSTANDARD2_1
                            o.OnNext(list.SkipLast(len).ToArray());
                            queue.Clear();
#else
                            o.OnNext(list.Take(list.Count - len).ToArray());
                            while (!queue.IsEmpty) queue.TryDequeue(out _);
#endif
                            list.Clear();
                        }
                        else
                        {
                            queue.TryDequeue(out _);
                        }
                    }
                }, o.OnCompleted);

                return Disposable.Create(() =>
                {
                    o.OnCompleted();
                    dis.Dispose();
                    list = null;
                    queue = null;
                });
            }).Where(p => p.Length > 0);
        }

        #endregion 分包处理
    }
}