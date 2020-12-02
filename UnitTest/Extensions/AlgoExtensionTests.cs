using AutoBogus;
using FluentAssertions;
using System;
using System.Diagnostics;
using System.Linq;
using Xunit;

namespace Limxc.Tools.Extensions.Tests
{
    public class AlgoExtensionTests
    {
        [Fact()]
        public void LocateTest()
        {
            var fakeLength = 4000;
            var bs = AutoFaker.Generate<byte>(fakeLength);

            //移除干扰数据
            var source = bs.ToArray();
            for (int i = 0; i < source.Length; i++)
            {
                if (source[i] == 0x90 || source[i] == 0xeb)
                    source[i] = 0x01;
            }
            source.Count(p => p == 0x90).Should().Be(0);

            //添加匹配项
            var pattern = new byte[] { 0xeb, 0x90 };
            var indexes = new int[] { 234, 1199, 2020, 3333, 3989 };
            var il = indexes.ToList();
            il.Sort();
            indexes = il.ToArray();

            foreach (var item in indexes)
            {
                bs.InsertRange(item, pattern);
            }
            source = bs.ToArray();

            //测试
            source.Length.Should().Be(fakeLength + indexes.Length * 2);

            var r = source.Locate1(pattern);
            var r0 = source.Locate<byte>(pattern);
            var r1 = source.Locate(pattern);
            var r2 = source.Locate_BM(pattern);
            var r3 = source.Locate_BM1(pattern);

            r.Should().BeEquivalentTo(r1).And.BeEquivalentTo(r2).And.BeEquivalentTo(r3);

            r.Should().BeEquivalentTo(indexes);
            r1.Should().BeEquivalentTo(indexes);
            r2.Should().BeEquivalentTo(indexes);
            r3.Should().BeEquivalentTo(indexes);

            //性能测试

            long PerformanceTest(Action action)
            {
                var sw = new Stopwatch();
                sw.Start();

                for (int i = 0; i < 50000; i++)
                {
                    action?.Invoke();
                }

                sw.Stop();
                return sw.ElapsedMilliseconds;
            }

            var t = PerformanceTest(() => source.Locate1(pattern));
            var t0 = PerformanceTest(() => source.Locate<byte>(pattern));
            var t1 = PerformanceTest(() => source.Locate(pattern));
            var t2 = PerformanceTest(() => source.Locate_BM(pattern));
            var t3 = PerformanceTest(() => source.Locate_BM1(pattern));

            Debugger.Break();
        }
    }
}