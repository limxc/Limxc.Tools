using Limxc.Tools.DeviceComm.Entities;
using Limxc.Tools.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

namespace Limxc.Tools.DeviceComm.Extensions
{
    public static class ProtocolExtension
    {
        /// <summary>
        /// 匹配解析返回值
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="resp"></param>
        /// <param name="byteToStringConverter">默认:<see cref="DataConverterExtension.ToHexStr"/></param>
        /// <returns></returns>
        public static IObservable<CPContext> FindResponse(this IObservable<CPContext> cmd, IObservable<byte[]> resp, Func<byte[], string> byteToStringConverter = null)
        {
            if (byteToStringConverter == null)
                byteToStringConverter = DataConverterExtension.ToHexStr;

            return cmd
                        .SelectMany(p =>
                        {
                            if (p.TimeOut == 0 || string.IsNullOrWhiteSpace(p.Response.Template) || p.SendTime == null)
                                return Observable.Return(p);

                            var st = ((DateTime)p.SendTime).ToDateTimeOffset();
                            return resp
                                        .Select(d => byteToStringConverter(d))
                                        .Timestamp()
                                        .SkipUntil(st)
                                        .TakeUntil(st.AddMilliseconds(p.TimeOut))
                                        .FirstOrDefaultAsync(t =>
                                        {
                                            return p.Response.Template.IsTemplateMatch(t.Value);
                                        })
                                        .Where(r => r.Value != null)
                                        .Select(r =>
                                        {
                                            p.Response.Value = r.Value;
                                            p.ReceivedTime = r.Timestamp.LocalDateTime;
                                            return p;
                                        })
                                        .DefaultIfEmpty(p)
                                        ;
                        });
        }

        public static IObservable<byte[]> B1E1MessagePackParser(this IObservable<byte> source, byte bom, byte eom)
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
        /// (不确定是否完整的包1)sep(完整包2)sep(完整包3)sep(未识别的包4)
        /// </summary>
        public class SeparatorMessagePack
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

        public static IObservable<byte[]> SeparatorMessagePackParser(this IObservable<byte> source, byte[] separator)
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
    }
}