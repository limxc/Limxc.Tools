﻿using AutoBogus;
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
            var pattern = new byte[] { 0xeb, 0x90 };

            int[] GetIndexes(int n)
            {
                var rnd = new Random(Guid.NewGuid().GetHashCode());
                var indexes = new List<int>();
                for (int i = 0; i < n; i++)
                {
                    var r = rnd.Next(1, 2000);
                    if (!indexes.Contains(r * 2))
                        indexes.Add(r * 2);
                }
                indexes.Sort();
                return indexes.ToArray();
            }
            byte[] GetTestData(int[] indexes)
            {
                //移除干扰数据
                var source = AutoFaker.Generate<byte>(fakeLength);
                for (int i = 0; i < source.Count; i++)
                {
                    if (source[i] == 0x90 || source[i] == 0xeb)
                        source[i] = 0x01;
                }
                source.Count(p => p == 0x90 || p == 0xeb).Should().Be(0);

                //添加匹配项
                foreach (var item in indexes)
                {
                    source.InsertRange(item, pattern);
                }
                source.Count.Should().Be(fakeLength + indexes.Length * 2);

                return source.ToArray();
            }

            var indexes = GetIndexes(5);
            var source = GetTestData(indexes);

            //测试
            void Test(int count)
            {
                var idx = GetIndexes(5);
                var data = GetTestData(idx);

                var r1 = data.Locate<byte>(pattern);
                var r2 = data.Locate(pattern);
                r1.Should().BeEquivalentTo(r2);
                r1.Should().BeEquivalentTo(idx);
            }
            for (int i = 0; i < 1000; i++)
            {
                Test(i);
            }

            //性能测试
            float PerformanceTest(Action action)
            {
                var sw = new Stopwatch();
                sw.Start();

                for (int i = 0; i < 50000; i++)
                {
                    action?.Invoke();
                }

                sw.Stop();

                return sw.ElapsedMilliseconds / 50000f;
            }

            var idx = GetIndexes(5);
            var data = GetTestData(idx);
            var t0 = PerformanceTest(() => data.Locate<byte>(pattern));
            var t1 = PerformanceTest(() => data.Locate(pattern));

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