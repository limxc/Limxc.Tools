using FluentAssertions;
using Force.DeepCloner;
using Limxc.Tools.DeviceComm.Entities;
using Microsoft.Reactive.Testing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnitTest.TestUtils;
using Xunit;

namespace Limxc.Tools.DeviceComm.Extensions.Tests
{
    public class ProtocolExtensionTests
    {
        [Fact()]
        public void B1E1ParsePackageTest()
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
                .ParsePackage(254, 255)
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
        public void ParsePackageTest()
        {
            var ts = new TestScheduler();
            //1bit
            var source1 = new byte[] {
                0,0,11,23,12,

                254,0,255,

                254,1,1,255,

                22,

                254,2,2,2,255

                ,33,5,0
            };
            var rst1 = new List<byte[]>();
            source1.ToObservable(ts)
                .ParsePackage(new byte[] { 254 }, new byte[] { 255 })
                .Subscribe(p => rst1.Add(p));

            ts.AdvanceBy(source1.Length);
            var expect1 = new List<byte[]>()
            {
                new byte[]{ 0 },
                new byte[]{ 1,1, },
                new byte[]{ 2,2,2 }
            };
            rst1.Should().BeEquivalentTo(expect1);

            //2bit
            var source2 = new byte[] {
                0,0,11,23,12,

                99,254,0,99,255,

                99,254,1,1,99,255,99,255,

                22,

                99,254,2,2,2,99,255,

                33,5,0
            };
            var rst2 = new List<byte[]>();
            source2.ToObservable(ts)
                .ParsePackage(new byte[] { 99, 254 }, new byte[] { 99, 255 })
                .Subscribe(p => rst2.Add(p));

            ts.AdvanceBy(source2.Length);
            var expect2 = new List<byte[]>()
            {
                new byte[]{ 0 },
                new byte[]{ 1,1, },
                new byte[]{ 2,2,2 }
            };
            rst2.Should().BeEquivalentTo(expect2);

            //3bit
            var source3 = new byte[] {
                0,0,11,23,12,

                88,99,254,0,88,99,255,

                88,99,254,1,1,88,99,255,

                22,

                99,254,2,2,2,88,99,255,

                88,99,254,2,2,2,88,99,255,

                33,5,0
            };
            var rst3 = new List<byte[]>();
            source3.ToObservable(ts)
                .ParsePackage(new byte[] { 88, 99, 254 }, new byte[] { 88, 99, 255 })
                .Subscribe(p => rst3.Add(p));

            ts.AdvanceBy(source3.Length);
            var expect3 = new List<byte[]>()
            {
                new byte[]{ 0 },
                new byte[]{ 1,1 },
                new byte[]{ 2,2,2 }
            };
            rst3.Should().BeEquivalentTo(expect3);

            //separator  98,98,97,97  bom = 97,97  eom = 98,98
            var sourceSep = new byte[] {
                2,41,
                97,97,0,98,98,//无法识别
                62,//Separator的缺陷
                97,97,1,1,98,98,//错误的包1: 2,41, 97,97,0,98,98, 62, 97,97,1,1,
                97,97,2,2,2,98,98,//包2: 2,2,2
                97,97,3,3,3,3,98//未识别: 3,3,3,3,98
            };
            var rstSep = new List<byte[]>();
            sourceSep.ToObservable(ts)
                .ParsePackage(new byte[] { 97, 97 }, new byte[] { 98, 98 })
                .Subscribe(p => rstSep.Add(p));

            ts.AdvanceBy(sourceSep.Length);
            var expectSep = new List<byte[]>()
            {
                new byte[]{ 0 },
                new byte[]{ 1,1 },
                new byte[]{ 2,2,2 }
            };
            rstSep.Should().BeEquivalentTo(expectSep);
            //separator 结果:  new byte[]{ 2,41, 97,97,0,98,98, 62, 97,97,1,1, }, new byte[] { 2, 2, 2 }
        }

        [Fact()]
        public void SeparatorParsePackageTest()
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
                .ParsePackage(new byte[] { 0, 0 })
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
                .ParsePackage(new byte[] { 0, 0, 0 })
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
                .ParsePackage(new byte[] { 98, 98, 97, 97 })
                .Subscribe(p => rst.Add(p));
            ts.AdvanceTo(d3.Length);
            rst.Should().BeEquivalentTo(new List<byte[]>
                                        {
                                            new byte[]{ 2,41, 97,97,0,98,98, 62, 97,97,1,1, },
                                            new byte[]{ 2,2,2 },
                                        });
            rst.Clear();
        }

        /// <summary>
        /// todo: 使用TestScheduler重构
        /// </summary>
        [Fact()]
        public async Task FindResponseTest()
        {
            var ts = new TestScheduler();

            var obsSend = Observable.Create<CPContext>(async o =>
                  {
                      //01  第1秒
                      await Task.Delay(1000);
                      o.OnNext(new CPContext("AA01BB", "AA$1BB", 1000, "01") { SendTime = DateTime.Now });

                      //02  第2秒
                      await Task.Delay(1000);
                      o.OnNext(new CPContext("AB02BB", "AB$1BB", 1000, "02") { SendTime = DateTime.Now });

                      //03  第3秒
                      await Task.Delay(1000);
                      o.OnNext(new CPContext("AC03BB", "AC$1BB", 0, "03") { SendTime = DateTime.Now });//不触发解析

                      //04  第4秒
                      await Task.Delay(1000);
                      o.OnNext(new CPContext("AD04BB", "AD$1BB", 1000, "04") { SendTime = DateTime.Now });

                      //05  第5秒
                      await Task.Delay(1000);
                      o.OnNext(new CPContext("AE05BB", "AE$1BB", 1000, "05") { SendTime = DateTime.Now });

                      //06  第6秒
                      await Task.Delay(1000);
                      o.OnNext(new CPContext("AF06BB", "AF$1BB", 1000, "06") { SendTime = DateTime.Now });

                      o.OnCompleted();
                      return Disposable.Empty;
                  })
                    .Publish()
                    //.RefCount();
                    ;

            var obsRecv = Observable.Create<byte[]>(async o =>
                    {
                        o.OnNext("AC0000AC".ToByte());
                        await Task.Delay(500);//500响应时间
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

            var cpsr = new List<CPContext>();

            //obsSend.Select(p => p.ToString())
            //    .Merge(obsRecv.Select(p => p.ToHexStr()))
            //    .Subscribe(p => Debug.WriteLine($"@{DateTime.Now:mm:ss fff} | {p}"));

            obsSend.FindResponse(obsRecv).Subscribe(p => cpsr.Add(p));

            var sc = obsSend.Connect();
            var rc = obsRecv.Connect();

            await Task.Delay(8000);

            cpsr[0].Status.Should().Be(CPContextStatus.Timeout);
            cpsr[1].Status.Should().Be(CPContextStatus.Success);
            cpsr[2].Status.Should().Be(CPContextStatus.NoNeed);
            cpsr[3].Status.Should().Be(CPContextStatus.Timeout);
            cpsr[4].Status.Should().Be(CPContextStatus.Success);
            cpsr[5].Status.Should().Be(CPContextStatus.Success);

            cpsr[1].Response.GetIntValues()[0].Should().Be(2);
            cpsr[4].Response.GetIntValues()[0].Should().Be(5);
            cpsr[5].Response.GetStrValues()[0].Should().Be("16");

            sc.Dispose();
            rc.Dispose();

            Debugger.Break();
        }

        [Fact()]
        public async void WaitingSendResultTest()
        {
            var simulator = new ProtocolSimulator(0, 3000);
            var msg = new List<string>();
            var rst = new List<CPContext>();

            simulator.Received.Select(p => $"@ {DateTime.Now:mm:ss fff} 接收 : {p.ToHexStr()}").Subscribe(p => msg.Add(p));
            simulator.History.Subscribe(p =>
            {
                msg.Add($"@ {DateTime.Now:mm:ss fff} {p}");
                rst.Add(p);
            });

            await simulator.OpenAsync();

            //有返回值
            var ctx1 = new CPTaskContext("1", "AA01BB", "AA$1BB", 1000 + 3000, 1);

            var begin = DateTime.Now;
            simulator.SendAsync(ctx1);
            await simulator.WaitingSendResult(ctx1, 1000 + 3000);
            var end = DateTime.Now;

            ctx1.Response.Value.Should().Be("AA01BB");
            (end - begin).TotalMilliseconds.Should().BeApproximately(3500, 500);
            rst.TrueForAll(p => p.Status == CPContextStatus.Success);

            rst.Clear();

            //无返回值
            var ctx2 = new CPTaskContext("2", "000000");
            simulator.SendAsync(ctx2);
            await simulator.WaitingSendResult(ctx2, 3000 + 3000);
            ctx2.Status.Should().Be(CPContextStatus.NoNeed);

            await simulator.CloseAsync();
            simulator.CleanUp();
        }

        [Fact()]
        public async void ExecQueueTest()
        {
            var simulator = new ProtocolSimulator();
            var msg = new List<string>();
            var history = new List<CPTaskContext>();

            simulator.Received.Select(p => $"@ {DateTime.Now:mm:ss fff} 接收 : {p.ToHexStr()}").Subscribe(p =>
            {
                msg.Add(p);
            });
            simulator.History.Subscribe(p =>
            {
                msg.Add($"@ {DateTime.Now:mm:ss fff} {p}");
                history.Add((p as CPTaskContext));
            });

            await simulator.OpenAsync();

            var tcList = new List<CPTaskContext>()
            {
                new CPTaskContext("id1", "AA01BB", "AA$1BB", 1000 , 3),
                new CPTaskContext("id2", "AB01BB", "AB$1BB", 1000 , 3),
                new CPTaskContext("id3", "AC01BB", "AA$1BB", 1000 , 3),//失败重试
                new CPTaskContext("id4", "AD01BB",  3),//无返回值
                new CPTaskContext("id5", "AE01BB", "AE$1BB", 1000 , 3)
            };

            await simulator.ExecQueue(tcList, CancellationToken.None, 2000);

            await simulator.CloseAsync();
            simulator.CleanUp();
 
            tcList.Count(p => p.Status == CPContextStatus.Timeout).Should().Be(1);
            tcList.Count(p => p.Status == CPContextStatus.NoNeed).Should().Be(1);
            tcList.Count(p => p.Status == CPContextStatus.Success).Should().Be(3);
              
            history.Count(p => p.Status == CPContextStatus.NoNeed).Should().Be(1);
            history.Count(p => p.Status == CPContextStatus.Success).Should().Be(3);
            history.Count(p => p.Status == CPContextStatus.Timeout).Should().Be(3);
        }
    }
}