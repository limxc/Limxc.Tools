using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Limxc.Tools.DeviceComm.Extensions;
using Limxc.Tools.DeviceComm.Protocol;
using Limxc.Tools.Entities.Communication;
using Limxc.Tools.Extensions.Communication;
using Microsoft.Reactive.Testing;
using Xunit;

namespace Limxc.ToolsTests.Extensions.Communication
{
    public class CommunicationExtensionTests
    {
        [Fact]
        public void B1E1ParsePackageTest()
        {
            var ts = new TestScheduler();

            var source = new byte[]
            {
                0, 0, 11, 23, 12,

                254, 0, 255,

                254, 1, 1, 255,

                22,

                254, 2, 2, 2, 255, 33, 5, 0
            };
            var rst = new List<byte[]>();
            source.ToObservable(ts)
                .ParsePackage<byte>(254, 255)
                .Subscribe(p => rst.Add(p));

            ts.AdvanceTo(source.Length);
            var expect = new List<byte[]>
            {
                new byte[] {0},
                new byte[] {1, 1},
                new byte[] {2, 2, 2}
            };
            rst.Should().BeEquivalentTo(expect);
        }

        [Fact]
        public void ParsePackageTest()
        {
            var ts = new TestScheduler();
            //1bit
            var source1 = new byte[]
            {
                0, 0, 11, 23, 12,

                254, 0, 255,

                254, 1, 1, 255,

                22,

                254, 2, 2, 2, 255, 33, 5, 0
            };
            var rst1 = new List<byte[]>();
            source1.ToObservable(ts)
                .ParsePackage(new byte[] {254}, new byte[] {255})
                .Subscribe(p => rst1.Add(p));

            ts.AdvanceBy(source1.Length);
            var expect1 = new List<byte[]>
            {
                new byte[] {0},
                new byte[] {1, 1},
                new byte[] {2, 2, 2}
            };
            rst1.Should().BeEquivalentTo(expect1);

            //2bit
            var source2 = new byte[]
            {
                0, 0, 11, 23, 12,

                99, 254, 0, 99, 255,

                99, 254, 1, 1, 99, 255, 99, 255,

                22,

                99, 254, 2, 2, 2, 99, 255,

                33, 5, 0
            };
            var rst2 = new List<byte[]>();
            source2.ToObservable(ts)
                .ParsePackage(new byte[] {99, 254}, new byte[] {99, 255})
                .Subscribe(p => rst2.Add(p));

            ts.AdvanceBy(source2.Length);
            var expect2 = new List<byte[]>
            {
                new byte[] {0},
                new byte[] {1, 1},
                new byte[] {2, 2, 2}
            };
            rst2.Should().BeEquivalentTo(expect2);

            //3bit
            var source3 = new byte[]
            {
                0, 0, 11, 23, 12,

                88, 99, 254, 0, 88, 99, 255,

                88, 99, 254, 1, 1, 88, 99, 255,

                22,

                99, 254, 2, 2, 2, 88, 99, 255,

                88, 99, 254, 2, 2, 2, 88, 99, 255,

                33, 5, 0
            };
            var rst3 = new List<byte[]>();
            source3.ToObservable(ts)
                .ParsePackage(new byte[] {88, 99, 254}, new byte[] {88, 99, 255})
                .Subscribe(p => rst3.Add(p));

            ts.AdvanceBy(source3.Length);
            var expect3 = new List<byte[]>
            {
                new byte[] {0},
                new byte[] {1, 1},
                new byte[] {2, 2, 2}
            };
            rst3.Should().BeEquivalentTo(expect3);

            //separator  98,98,97,97  bom = 97,97  eom = 98,98
            var sourceSep = new byte[]
            {
                2, 41,
                97, 97, 0, 98, 98, //无法识别
                62, //Separator的缺陷
                97, 97, 1, 1, 98, 98, //错误的包1: 2,41, 97,97,0,98,98, 62, 97,97,1,1,
                97, 97, 2, 2, 2, 98, 98, //包2: 2,2,2
                97, 97, 3, 3, 3, 3, 98 //未识别: 3,3,3,3,98
            };
            var rstSep = new List<byte[]>();
            sourceSep.ToObservable(ts)
                .ParsePackage(new byte[] {97, 97}, new byte[] {98, 98})
                .Subscribe(p => rstSep.Add(p));

            ts.AdvanceBy(sourceSep.Length);
            var expectSep = new List<byte[]>
            {
                new byte[] {0},
                new byte[] {1, 1},
                new byte[] {2, 2, 2}
            };
            rstSep.Should().BeEquivalentTo(expectSep);
            //separator 结果:  new byte[]{ 2,41, 97,97,0,98,98, 62, 97,97,1,1, }, new byte[] { 2, 2, 2 }
        }

        [Fact]
        public void GenericParsePackageTest()
        {
            var ts = new TestScheduler();
            var rst = new List<string>();

            var d1 = "A*stp=1B" + "error" + "A41.2B" + "A41.9B" + "error" + "A46.0B" + "A*stp=2B" + "error";
            var dis1 = d1.ToObservable(ts)
                .ParsePackage('A', 'B')
                .Select(p => new string(p))
                .Subscribe(p => rst.Add(p));
            ts.AdvanceBy(d1.Length);
            rst.Should().BeEquivalentTo(new List<string>
            {
                "*stp=1",
                "41.2",
                "41.9",
                "46.0",
                "*stp=2"
            });
            dis1.Dispose();
            rst.Clear();

            var d2 = "AA*stp=1BB" + "error" + "AA41.2BB" + "AA41.9BB" + "error" + "AA46.0BB" + "AA*stp=2BB" + "error";

            var dis2 = d2.ToObservable(ts)
                .ParsePackage("AA".ToCharArray(), "BB".ToCharArray())
                .Select(p => new string(p))
                .Subscribe(p => rst.Add(p));
            ts.AdvanceBy(d2.Length);
            rst.Should().BeEquivalentTo(new List<string>
            {
                "*stp=1",
                "41.2",
                "41.9",
                "46.0",
                "*stp=2"
            });
            dis2.Dispose();
            rst.Clear();
        }

        [Fact]
        public void SeparatorParsePackageTest()
        {
            var ts = new TestScheduler();

            var d1 = new byte[]
            {
                1, 0, 8, //不确定是否完整的包1
                0, 0,
                11, 52, 55, //包2
                0, 0,
                0, 255, 33, 5 //未识别
            };

            var d2 = new byte[]
            {
                0, 0, 0, //空包1, 过滤
                0, 11, 254, 0, 255, 254, //包2
                0, 0, 0,
                255, 22, 254, //包3
                0, 0, 0,
                0, 255, 33, 5, 0 //未识别
            };

            //bom aa(97,97)  eom bb(98,98)  sep: 98,98,97,97
            var d3 = new byte[]
            {
                2, 41,
                97, 97, 0, 98, 98, //无法识别
                62, //Separator的缺陷
                97, 97, 1, 1, 98, 98, //错误的包1: 2,41, 97,97,0,98,98, 62, 97,97,1,1,
                97, 97, 2, 2, 2, 98, 98, //包2: 2,2,2
                97, 97, 3, 3, 3, 3, 98 //未识别: 3,3,3,3,98
            };

            var rst = new List<byte[]>();
            //d1
            d1.ToObservable(ts)
                .ParsePackage(new byte[] {0, 0})
                .Subscribe(p => rst.Add(p));
            ts.AdvanceBy(d1.Length);
            rst.Should().BeEquivalentTo(new List<byte[]>
            {
                new byte[] {1, 0, 8},
                new byte[] {11, 52, 55}
            });
            rst.Clear();

            //d2
            d2.ToObservable(ts)
                .ParsePackage(new byte[] {0, 0, 0})
                .Subscribe(p => rst.Add(p));
            ts.AdvanceBy(d2.Length);
            rst.Should().BeEquivalentTo(new List<byte[]>
            {
                new byte[] {0, 11, 254, 0, 255, 254},
                new byte[] {255, 22, 254}
            });
            rst.Clear();

            //d3
            d3.ToObservable(ts)
                .ParsePackage(new byte[] {98, 98, 97, 97})
                .Subscribe(p => rst.Add(p));
            ts.AdvanceBy(d3.Length);
            rst.Should().BeEquivalentTo(new List<byte[]>
            {
                new byte[] {2, 41, 97, 97, 0, 98, 98, 62, 97, 97, 1, 1},
                new byte[] {2, 2, 2}
            });
            rst.Clear();
        }

        [Fact]
        public void GenericSeparatorParsePackageTest()
        {
            var ts = new TestScheduler();

            var data = "\n*stp=1\n41.2\n41.9\n46.0\n*stp=2\nerror";

            var rst = new List<string>();

            data.ToObservable()
                .ParsePackage("\n".ToCharArray())
                .Select(p => new string(p))
                .Subscribe(p => rst.Add(p));
            ts.AdvanceTo(data.Length);
            rst.Should().BeEquivalentTo(new List<string>
            {
                "*stp=1",
                "41.2",
                "41.9",
                "46.0",
                "*stp=2"
            });
        }

        /// <summary>
        ///     todo: 使用TestScheduler重构
        /// </summary>
        [Fact]
        public async Task FindResponseTest()
        {
            var obsSend = Observable.Create<CommContext>(async o =>
                    {
                        //01  第1秒
                        await Task.Delay(1000);
                        o.OnNext(new CommContext("AA01BB", "AA$1BB", 1000, "01") {SendTime = DateTime.Now});

                        //02  第2秒
                        await Task.Delay(1000);
                        o.OnNext(new CommContext("AB02BB", "AB$1BB", 1000, "02") {SendTime = DateTime.Now});

                        //03  第3秒
                        await Task.Delay(1000);
                        o.OnNext(new CommContext("AC03BB", "AC$1BB", 0, "03") {SendTime = DateTime.Now}); //不触发解析

                        //04  第4秒
                        await Task.Delay(1000);
                        o.OnNext(new CommContext("AD04BB", "AD$1BB", 1000, "04") {SendTime = DateTime.Now});

                        //05  第5秒
                        await Task.Delay(1000);
                        o.OnNext(new CommContext("AE05BB", "AE$1BB", 1000, "05") {SendTime = DateTime.Now});

                        //06  第6秒
                        await Task.Delay(1000);
                        o.OnNext(new CommContext("AF06BB", "AF$1BB", 1000, "06") {SendTime = DateTime.Now});

                        o.OnCompleted();
                        return Disposable.Empty;
                    })
                    .Publish()
                //.RefCount();
                ;

            var obsRecv = Observable.Create<byte[]>(async o =>
                    {
                        o.OnNext("AC0000AC".ToByte());
                        await Task.Delay(500); //500响应时间
                        //01 丢失
                        //第2.5秒 ; 02 接收
                        await Task.Delay(2000);
                        o.OnNext("AB02BB".ToByte());

                        //第3.5秒 ; 03 不匹配
                        await Task.Delay(1000);
                        o.OnNext("A003BB".ToByte());

                        //第5.5秒 ; 04 超时 ; 05 接收
                        await Task.Delay(2000);
                        o.OnNext("AD04BB".ToByte());
                        o.OnNext("AE05BB".ToByte());

                        //第6.5秒 ; 06接收第一个
                        await Task.Delay(1000);
                        o.OnNext("AF16BB".ToByte());
                        o.OnNext("AF26BB".ToByte());

                        o.OnCompleted();
                        return Disposable.Empty;
                    })
                    .Publish()
                //.RefCount()
                ;

            var cpsr = new List<CommContext>();

            //obsSend.Select(p => p.ToString())
            //    .Merge(obsRecv.Select(p => p.ToHexStr()))
            //    .Subscribe(p => Debug.WriteLine($"@{DateTime.Now:mm:ss fff} | {p}"));

            obsSend.FindResponse(obsRecv).Subscribe(p => cpsr.Add(p));

            var sc = obsSend.Connect();
            var rc = obsRecv.Connect();

            await Task.Delay(8000);
            cpsr[0].State.Should().Be(CommContextState.Timeout);
            cpsr[1].State.Should().Be(CommContextState.Success);
            cpsr[2].State.Should().Be(CommContextState.NoNeed);
            cpsr[3].State.Should().Be(CommContextState.Timeout);
            cpsr[4].State.Should().Be(CommContextState.Success);
            cpsr[5].State.Should().Be(CommContextState.Success);

            cpsr[1].Response.GetIntValues()[0].Should().Be(2);
            cpsr[4].Response.GetIntValues()[0].Should().Be(5);
            cpsr[5].Response.GetStrValues()[0].Should().Be("16");

            //sc.Dispose();
            //rc.Dispose();

            Debugger.Break();
        }

        [Fact]
        public async Task WaitingSendResultTest()
        {
            var simulator = new ProtocolSimulator(0, 3000);
            var msg = new List<string>();
            var rst = new List<CommContext>();

            simulator.Received.Select(p => $"@ {DateTime.Now:mm:ss fff} 接收 : {p.ToHexStr()}")
                .Subscribe(p => msg.Add(p));
            simulator.History.Subscribe(p =>
            {
                msg.Add($"@ {DateTime.Now:mm:ss fff} {p}");
                rst.Add(p);
            });

            await simulator.OpenAsync();

            //有返回值
            var ctx1 = new CommTaskContext("1", "AA01BB", "AA$1BB", 1000 + 3000);

            var begin = DateTime.Now;
            _ = simulator.SendAsync(ctx1);
            await simulator.WaitingSendResult(ctx1, 1000 + 3000);
            var end = DateTime.Now;

            ctx1.Response.Value.Should().Be("AA01BB");
            (end - begin).TotalMilliseconds.Should().BeApproximately(3500, 500);
            rst.TrueForAll(p => p.State == CommContextState.Success);

            rst.Clear();

            //无返回值
            var ctx2 = new CommTaskContext("2", "000000");
            _ = simulator.SendAsync(ctx2);
            await simulator.WaitingSendResult(ctx2, 3000 + 3000);
            ctx2.State.Should().Be(CommContextState.NoNeed);

            await simulator.CloseAsync();
            simulator.Dispose();
        }

        [Fact]
        public async Task ExecQueueTest()
        {
            var simulator = new ProtocolSimulator();
            var msg = new List<string>();
            var history = new List<CommTaskContext>();

            simulator.Received.Select(p => $"@ {DateTime.Now:mm:ss fff} 接收 : {p.ToHexStr()}").Subscribe(p =>
            {
                msg.Add(p);
            });
            simulator.History.Subscribe(p =>
            {
                msg.Add($"@ {DateTime.Now:mm:ss fff} {p}");
                history.Add(p as CommTaskContext);
            });

            await simulator.OpenAsync();

            var tcList = new List<CommTaskContext>
            {
                new("id1", "AA01BB", "AA$1BB", 1000, 3),
                new("id2", "AB01BB", "AB$1BB", 1000, 3),
                new("id3", "AC01BB", "AA$1BB", 1000, 3), //失败重试
                new("id4", "AD01BB", 3), //无返回值
                new("id5", "AE01BB", "AE$1BB", 1000, 3)
            };

            await simulator.ExecQueue(tcList, CancellationToken.None, 2000);

            await simulator.CloseAsync();
            simulator.Dispose();

            tcList.Count(p => p.State == CommContextState.Timeout).Should().Be(1);
            tcList.Count(p => p.State == CommContextState.NoNeed).Should().Be(1);
            tcList.Count(p => p.State == CommContextState.Success).Should().Be(3);

            history.Count(p => p.State == CommContextState.NoNeed).Should().Be(1);
            history.Count(p => p.State == CommContextState.Success).Should().Be(3);
            history.Count(p => p.State == CommContextState.Timeout).Should().Be(3);
        }
    }
}