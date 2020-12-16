using Limxc.Tools.DeviceComm.Entities;
using Limxc.Tools.DeviceComm.Protocol;
using Limxc.Tools.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Limxc.Tools.DeviceComm.Extensions
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
                        return Observable.Return(p);//.Delay(TimeSpan.FromMilliseconds(500));
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

        #region 包头包尾分包

        /// <summary>
        /// 字节流分包 (1bit头 1bit尾)
        /// </summary>
        /// <param name="source"></param>
        /// <param name="bom"></param>
        /// <param name="eom"></param>
        /// <returns></returns>
        public static IObservable<byte[]> ParsePackage(this IObservable<byte> source, byte bom, byte eom)
        {
            return source.Publish(s =>
            {
                return Observable.Defer
                (() => s
                        .SkipWhile(b => b != bom)
                        .Skip(1)
                        .TakeWhile(b => b != eom)
                        .ToArray()
                        .Where(p => p.Any())
                        .Repeat()
                );
            });
        }

        /// <summary>
        /// 字节流分包
        /// </summary>
        /// <param name="source"></param>
        /// <param name="bom"></param>
        /// <param name="eom"></param>
        /// <returns></returns>
        public static IObservable<byte[]> ParsePackage(this IObservable<byte> source, byte[] bom, byte[] eom)
        {
            if (bom.Length == 0 || eom.Length == 0)
                throw new ArgumentException("If bom or eom is empty, use SeparatorMessagePackParser.");
            return source.Publish((Func<IObservable<byte>, IObservable<byte[]>>)(s =>
            {
                return Observable.Defer
                ((Func<IObservable<byte[]>>)(() => s

                        /* 项目“Limxc.Tools.DeviceComm (netstandard2.1)”的未合并的更改
                        在此之前:
                                                .Scan(new BEMessagePack(bom, eom, new byte[0]), (pack, b) => pack.Push(b))
                        在此之后:
                                                .Scan(new ProtocolExtension.BEMessagePack(bom, eom, new byte[0]), (pack, b) => pack.Push(b))
                        */
                        .Scan((MessagePack)new MessagePack(bom, eom, new byte[0]), (pack, b) => pack.Push(b))
                        .Where(p => p.IsComplete && p.AccumulatedBytes.Any())
                        .Select(p => p.AccumulatedBytes.ToArray()))
                );
            }));
        }

        private class MessagePack
        {
            public byte[] Bom { get; }
            public byte[] Eom { get; }

            public MessagePack(byte[] bom, byte[] eom, IEnumerable<byte> accumulatedBytes)
            {
                Bom = bom;
                Eom = eom;
                AccumulatedBytes = accumulatedBytes;
            }

            public bool IsComplete { get; private set; }
            public IEnumerable<byte> AccumulatedBytes { get; private set; }
            public bool IsBomComplete { get; private set; }

            public MessagePack Push(byte b)
            {
                if (IsComplete == true)
                {
                    IsBomComplete = false;
                    IsComplete = false;
                    AccumulatedBytes = new byte[0];
                }
                AccumulatedBytes = AccumulatedBytes.Concat(new[] { b });

                var abLen = AccumulatedBytes.Count();
                var bomLen = Bom.Length;
                var eomLen = Eom.Length;
                //head
                if (!IsBomComplete && abLen == bomLen)
                {
                    var arr = AccumulatedBytes.ToArray();
                    for (int i = 0; i < AccumulatedBytes.Count(); i++)
                    {
                        if (arr[i] != Bom[i])
                            return new MessagePack(Bom, Eom, AccumulatedBytes.Skip(i + 1));
                    }

                    IsBomComplete = true;
                }
                //tail
                if (IsBomComplete && abLen >= bomLen + eomLen && AccumulatedBytes.Skip(abLen - eomLen).SequenceEqual(Eom))
                {
                    return new MessagePack(Bom, Eom, AccumulatedBytes.Skip(bomLen).Take(abLen - bomLen - eomLen)) { IsComplete = true, IsBomComplete = IsBomComplete };
                }

                return new MessagePack(Bom, Eom, AccumulatedBytes) { IsBomComplete = IsBomComplete };
            }
        }

        #endregion 包头包尾分包

        #region 分隔符分包

        /// <summary>
        /// 字节流分包 (不确定是否完整的包1|完整包2|...|完整包3|未识别的包4)
        /// </summary>
        public static IObservable<byte[]> ParsePackage(this IObservable<byte> source, byte[] separator)
        {
            return source.Publish(s =>
            {
                return Observable.Defer
                (() => s
                        .Scan(new SeparatorMessagePack(separator, new byte[0]), (pack, b) => pack.Push(b))
                        .Where(p => p.IsComplete && p.AccumulatedBytes.Any())
                        .Select(p => p.AccumulatedBytes.ToArray())
                );
            });
        }

        /// <summary>
        ///  (不确定是否完整的包1)sep(完整包2)sep...sep(完整包3)sep(未识别的包4)
        /// </summary>
        private class SeparatorMessagePack
        {
            public byte[] Separator { get; }

            public SeparatorMessagePack(byte[] separator, IEnumerable<byte> accumulatedBytes)
            {
                Separator = separator;

                AccumulatedBytes = accumulatedBytes;
            }

            public bool IsComplete { get; private set; }
            public IEnumerable<byte> AccumulatedBytes { get; private set; }

            public SeparatorMessagePack Push(byte b)
            {
                if (IsComplete == true)
                {
                    IsComplete = false;
                    AccumulatedBytes = new byte[0];
                }
                AccumulatedBytes = AccumulatedBytes.Concat(new[] { b });

                var abLen = AccumulatedBytes.Count();
                var sepLen = Separator.Length;
                if (abLen >= sepLen && AccumulatedBytes.Skip(abLen - sepLen).SequenceEqual(Separator))
                {
                    return new SeparatorMessagePack(Separator, AccumulatedBytes.Take(abLen - sepLen)) { IsComplete = true };
                }
                return new SeparatorMessagePack(Separator, AccumulatedBytes);
            }
        }

        #endregion 分隔符分包

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
            int state = 0;//等待中..

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