using FluentAssertions;
using Microsoft.Reactive.Testing;
using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using Xunit;

namespace Limxc.Tools.DeviceComm.Extensions.Tests
{
    public class ProtocolExtensionTests
    {
        [Fact()]
        public void B1E1MessagePackParserTest()
        {
            var ts = new TestScheduler();

            var source = new byte[] {
                0,0,11,23,12,

                254,0,255,

                254,1,1,255,

                22,

                254,2,2,2,255

                ,33,5,0
            };
            var rst = new List<byte[]>();
            source.ToObservable(ts)
                .B1E1MessagePackParser(254, 255)
                .Subscribe(p => rst.Add(p));

            ts.AdvanceTo(source.Length);
            var expect = new List<byte[]>()
            {
                new byte[]{ 0 },
                new byte[]{ 1,1, },
                new byte[]{ 2,2,2 }
            };
            rst.Should().BeEquivalentTo(expect);
        }

        [Fact()]
        public void SeparratorMessagePackParserTest()
        {
            var ts = new TestScheduler();

            var d1 = new byte[] {
                1,0,8,//不确定是否完整的包1
                0,0,
                11,52,55,//包2
                0,0,
                0,255,33,5,//未识别
            };

            var d2 = new byte[] {
                0,0,0,//空包1, 过滤
                0,11,254,0,255,254,//包2
                0,0,0,
                255,22,254,//包3
                0,0,0,
                0,255,33,5,0//未识别
            };

            //bom aa(97,97)  eom bb(98,98)  sep: 98,98,97,97
            var d3 = new byte[] {
                2,41,
                97,97,0,98,98,//无法识别
                62,//Separator的缺陷
                97,97,1,1,98,98,//错误的包1: 2,41, 97,97,0,98,98, 62, 97,97,1,1,
                97,97,2,2,2,98,98,//包2: 2,2,2
                97,97,3,3,3,3,98//未识别: 3,3,3,3,98
            };

            var rst = new List<byte[]>();
            //d1
            d1.ToObservable()
                .SeparatorMessagePackParser(new byte[] { 0, 0 })
                .Subscribe(p => rst.Add(p));
            ts.AdvanceTo(d1.Length);
            rst.Should().BeEquivalentTo(new List<byte[]>
                                        {
                                            new byte[]{ 1,0,8 },
                                            new byte[]{ 11,52,55 },
                                        });
            rst.Clear();

            //d2
            d2.ToObservable()
                .SeparatorMessagePackParser(new byte[] { 0, 0, 0 })
                .Subscribe(p => rst.Add(p));
            ts.AdvanceTo(d2.Length);
            rst.Should().BeEquivalentTo(new List<byte[]>
                                        {
                                            new byte[]{ 0,11,254,0,255,254 },
                                            new byte[]{ 255,22,254 },
                                        });
            rst.Clear();

            //d3
            d3.ToObservable()
                .SeparatorMessagePackParser(new byte[] { 98, 98, 97, 97 })
                .Subscribe(p => rst.Add(p));
            ts.AdvanceTo(d3.Length);
            rst.Should().BeEquivalentTo(new List<byte[]>
                                        {
                                            new byte[]{ 2,41, 97,97,0,98,98, 62, 97,97,1,1, },
                                            new byte[]{ 2,2,2 },
                                        });
            rst.Clear();
        }
    }
}