using Limxc.Tools.DeviceComm.Protocol;
using Limxc.Tools.Entities.DevComm;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Limxc.Tools.Extensions.DevComm
{
    public static class ProtocolExtension
    {
        /// <summary>
        /// 返回值匹配及解析
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="resp"></param>
        /// <param name="byteToStringConverter">默认:<see cref="DataConversionExtension.ToHexStr"/></param>
        /// <returns></returns>
        public static IObservable<CPContext> FindResponse(this IObservable<CPContext> cmd, IObservable<byte[]> resp, Func<byte[], string> byteToStringConverter = null)
        {
            if (byteToStringConverter == null)
                byteToStringConverter = DataConversionExtension.ToHexStr;

            return cmd
                .SelectMany(p =>
                {
                    if (p.Timeout == 0 || string.IsNullOrWhiteSpace(p.Response.Template))
                    {
                        p.State = CPContextState.NoNeed;
                        return Observable.Return(p);
                    }

                    var st = ((DateTime)p.SendTime).ToDateTimeOffset();

                    return resp.Timestamp()
                                .Select(d => new Timestamped<string>(byteToStringConverter(d.Value), d.Timestamp))
                                .SkipUntil(st)
                                .TakeUntil(st.AddMilliseconds(p.Timeout))
                                .FirstOrDefaultAsync(t => p.Response.Template.IsTemplateMatch(t.Value))
                                .Select(r =>
                                {
                                    if (r.Value != null)
                                    {
                                        p.Response.Value = r.Value;
                                        p.ReceivedTime = r.Timestamp.LocalDateTime;
                                        p.State = CPContextState.Success;
                                    }
                                    else
                                    {
                                        p.State = CPContextState.Timeout;
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
        /// 1包头1包尾分包
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
        /// 包头包尾分包
        /// </summary>
        /// <param name="source"></param>
        /// <param name="bom"></param>
        /// <param name="eom"></param>
        /// <returns></returns>
        public static IObservable<T[]> ParsePackage<T>(this IObservable<T> source, T[] bom, T[] eom) where T : IEquatable<T>
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
                                while (bQueue.TryDequeue(out _)) { }
                                while (eQueue.TryDequeue(out _)) { }
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
                }, () => o.OnCompleted());

                return Disposable.Create(() =>
                {
                    o.OnCompleted();
                    dis.Dispose();
                    list.Clear();
#if NETSTANDARD2_1
                    bQueue.Clear();
                    eQueue.Clear();
#else
                    while (bQueue.TryDequeue(out _)) { }
                    while (eQueue.TryDequeue(out _)) { }
#endif
                    list = null;
                    bQueue = null;
                    eQueue = null;
                });
            });
        }

        /// <summary>
        /// 分隔符分包
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="separator"></param>
        /// <returns></returns>
        public static IObservable<T[]> ParsePackage<T>(this IObservable<T> source, T[] separator) where T : IEquatable<T>
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
                            while (queue.TryDequeue(out _)) { }
#endif
                            list.Clear();
                        }
                        else
                        {
                            queue.TryDequeue(out _);
                        }
                    }
                }, () => o.OnCompleted());

                return Disposable.Create(() =>
                {
                    o.OnCompleted();
                    dis.Dispose();
                    list.Clear();
#if NETSTANDARD2_1
                    queue.Clear();
#else
                    while (queue.TryDequeue(out _)) { }
#endif
                    list = null;
                    queue = null;
                });
            }).Where(p => p.Length > 0);
        }

        #endregion 分包处理

        #region 任务队列

        /// <summary>
        /// Send解析任务完成通知
        /// </summary>
        /// <param name="protocol"></param>
        /// <param name="context"></param>
        /// <param name="schedulerRunTime">rx处理时间</param>
        /// <returns></returns>
        public static async Task WaitingSendResult(this IProtocol protocol, CPTaskContext context, int schedulerRunTime = 100)
        {
            var state = 0;//等待中..

            var dis = protocol
                        .History
                        .TakeUntil(DateTimeOffset.Now.AddMilliseconds(context.Timeout + schedulerRunTime))
                        .FirstOrDefaultAsync(p => ((CPTaskContext)p).Id == context.Id)
                        .ObserveOn(TaskPoolScheduler.Default)
                        .Subscribe(p =>
                        {
                            if (p == null)
                                state = 1;//返回值丢失
                            else
                                state = 2;//有返回值
                        });

            await protocol.SendAsync(context).ConfigureAwait(false);

            while (state == 0)
            {
                await Task.Delay(10).ConfigureAwait(false);
            }

            dis.Dispose();

            if (state == 1)
                throw new Exception($"Send Result Lost. {nameof(CPTaskContext.Id)}:{context.Id}");
        }

        /// <summary>
        /// 任务队列执行
        /// </summary>
        /// <param name="protocol"></param>
        /// <param name="queue"></param>
        /// <param name="token"></param>
        /// <param name="schedulerRunTime">rx处理时间</param>
        /// <returns></returns>
        public static async Task ExecQueue(this IProtocol protocol, List<CPTaskContext> queue, CancellationToken token, int schedulerRunTime = 100)
        {
            foreach (var task in queue)
            {
                while (!token.IsCancellationRequested && task.State != CPContextState.Success && task.State != CPContextState.NoNeed && task.RemainTimes > 0)
                {
                    task.RemainTimes--;
                    await protocol.WaitingSendResult(task, schedulerRunTime).ConfigureAwait(false);
                }
            }
            return;
        }

        #endregion 任务队列
    }
}