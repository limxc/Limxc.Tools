using AutoBogus;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Xunit;

namespace Limxc.Tools.Extensions.Tests
{
    public class AlgoExtensionTests
    {
        [Fact()]
        public void LocateByteTest()
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
            source.Count(p => p == 0x90 || p == 0xeb).Should().Be(0);

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

            var r0 = source.Locate<byte>(pattern);
            var r1 = source.Locate(pattern);

            r0.Should().BeEquivalentTo(r1);

            r0.Should().BeEquivalentTo(indexes);
            r1.Should().BeEquivalentTo(indexes);

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

            var t0 = PerformanceTest(() => source.Locate<byte>(pattern));
            var t1 = PerformanceTest(() => source.Locate(pattern));

            Debugger.Break();
        }

        [Fact()]
        public void LocateStringTest()
        {
            var fakeLength = 4000;
            var bs = AutoFaker.Generate<char>(fakeLength);

            //移除干扰数据
            var source = new string(bs.ToArray());
            source = source.Replace('a', '0').Replace('b', '0');

            //添加匹配项
            var pattern = "ab";
            var indexes = new int[] { 234, 1199, 2020, 3333, 3989 };
            var il = indexes.ToList();
            il.Sort();
            indexes = il.ToArray();

            foreach (var index in indexes)
            {
                source = source.Insert(index, pattern);
            }

            //测试
            source.Length.Should().Be(fakeLength + indexes.Length * 2);

            var r0 = source.ToCharArray().Locate<char>(pattern.ToCharArray());
            var r1 = source.Locate(new string(pattern));

            r0.Should().BeEquivalentTo(r1);

            r0.Should().BeEquivalentTo(indexes);
            r1.Should().BeEquivalentTo(indexes);

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

            var t0 = PerformanceTest(() => source.ToCharArray().Locate<char>(pattern.ToCharArray()));
            var t1 = PerformanceTest(() => source.Locate(pattern));

            Debugger.Break();
        }

        [Fact()]
        public void LocateToPackGenericTest()
        {
            var datas = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };

            new int[] { }.LocateToPack(datas).Should().BeEquivalentTo((new byte[][] { }, new byte[] { }));

            Action act = () => new int[] { 11 }.LocateToPack(datas);
            act.Should().Throw<ArgumentException>();
            Assert.Throws<ArgumentException>(() => { new int[] { -1 }.LocateToPack(datas); });

            var r1 = new int[] { 9 }.LocateToPack(datas);
            r1.pack.Should().BeEquivalentTo(new List<byte[]>().ToArray());
            r1.remain.Should().BeEquivalentTo(new byte[] { 9 });

            new int[] { 0 }.LocateToPack(datas).Should().BeEquivalentTo((
                                                                            new byte[][] { },
                                                                            new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }
                                                                          ));

            new int[] { 0, 9 }.LocateToPack(datas).Should().BeEquivalentTo((
                                                                                new byte[][]
                                                                                    {
                                                                                        new byte[]{ 0,1,2,3,4,5,6,7,8 }
                                                                                    },
                                                                                new byte[] { 9 }
                                                                             ));

            new int[] { 1, 3, 6 }.LocateToPack(datas).Should().BeEquivalentTo((
                                                                                    new byte[][]
                                                                                    {
                                                                                        new byte[]{ 1,2 },
                                                                                        new byte[]{ 3,4,5 }
                                                                                    },
                                                                                    new byte[] { 6, 7, 8, 9 }
                                                                                ));
        }

        [Fact()]
        public void LocateToPackTest()
        {
            var pattern1 = new byte[] { 0xa };
            var pattern2 = new byte[] { 0xa, 0xb };

            new byte[] { 0, 1, 0xa, 3, 4, 0xa, 6, 7, 8, 9 }
                .LocateToPack(pattern1).Should().BeEquivalentTo((
                                                                    new byte[][]
                                                                        {
                                                                            new byte[]{ 0xa,3,4 },
                                                                        },
                                                                    new byte[] { 0xa, 6, 7, 8, 9 }
                                                                  ));

            new byte[] { 0, 1, 0xa, 0xb, 3, 4, 0xa, 6, 7, 8, 9, 0xa }
                .LocateToPack(pattern1).Should().BeEquivalentTo((
                                                                    new byte[][]
                                                                        {
                                                                            new byte[] { 0xa, 0xb, 3, 4 },
                                                                            new byte[] { 0xa, 6, 7, 8, 9 }
                                                                        },
                                                                    new byte[] { 0xa }
                                                                  ));

            new byte[] { 0, 1, 0xa, 0xb, 3, 4, 0xa, 6, 7, 8, 9 }
                .LocateToPack(pattern2).Should().BeEquivalentTo((
                                                                    new byte[][]
                                                                        {
                                                                        },
                                                                    new byte[] { 0xa, 0xb, 3, 4, 0xa, 6, 7, 8, 9 }
                                                                  ));
        }
    }
}