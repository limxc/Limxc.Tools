using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Limxc.Tools.Bases.Communication;
using Limxc.Tools.Extensions.Communication;
using Microsoft.Reactive.Testing;
using Xunit;

namespace Limxc.ToolsTests.Extensions.Communication;

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
            new byte[] { 0 },
            new byte[] { 1, 1 },
            new byte[] { 2, 2, 2 }
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
            .ParsePackage(new byte[] { 254 }, new byte[] { 255 })
            .Subscribe(p => rst1.Add(p));

        ts.AdvanceBy(source1.Length);
        var expect1 = new List<byte[]>
        {
            new byte[] { 0 },
            new byte[] { 1, 1 },
            new byte[] { 2, 2, 2 }
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
            .ParsePackage(new byte[] { 99, 254 }, new byte[] { 99, 255 })
            .Subscribe(p => rst2.Add(p));

        ts.AdvanceBy(source2.Length);
        var expect2 = new List<byte[]>
        {
            new byte[] { 0 },
            new byte[] { 1, 1 },
            new byte[] { 2, 2, 2 }
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
            .ParsePackage(new byte[] { 88, 99, 254 }, new byte[] { 88, 99, 255 })
            .Subscribe(p => rst3.Add(p));

        ts.AdvanceBy(source3.Length);
        var expect3 = new List<byte[]>
        {
            new byte[] { 0 },
            new byte[] { 1, 1 },
            new byte[] { 2, 2, 2 }
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
            .ParsePackage(new byte[] { 97, 97 }, new byte[] { 98, 98 })
            .Subscribe(p => rstSep.Add(p));

        ts.AdvanceBy(sourceSep.Length);
        var expectSep = new List<byte[]>
        {
            new byte[] { 0 },
            new byte[] { 1, 1 },
            new byte[] { 2, 2, 2 }
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
            .ParsePackage(new byte[] { 0, 0 })
            .Subscribe(p => rst.Add(p));
        ts.AdvanceBy(d1.Length);
        rst.Should().BeEquivalentTo(new List<byte[]>
        {
            new byte[] { 1, 0, 8 },
            new byte[] { 11, 52, 55 }
        });
        rst.Clear();

        //d2
        d2.ToObservable(ts)
            .ParsePackage(new byte[] { 0, 0, 0 })
            .Subscribe(p => rst.Add(p));
        ts.AdvanceBy(d2.Length);
        rst.Should().BeEquivalentTo(new List<byte[]>
        {
            new byte[] { 0, 11, 254, 0, 255, 254 },
            new byte[] { 255, 22, 254 }
        });
        rst.Clear();

        //d3
        d3.ToObservable(ts)
            .ParsePackage(new byte[] { 98, 98, 97, 97 })
            .Subscribe(p => rst.Add(p));
        ts.AdvanceBy(d3.Length);
        rst.Should().BeEquivalentTo(new List<byte[]>
        {
            new byte[] { 2, 41, 97, 97, 0, 98, 98, 62, 97, 97, 1, 1 },
            new byte[] { 2, 2, 2 }
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

    [Fact]
    public async Task FindResponseTest()
    {
        var obsSend = Observable.Create<CommContext>(async o =>
                {
                    //01  第1秒
                    await Task.Delay(1000);
                    o.OnNext(new CommContext("AA01BB", "AA$1BB", 800, "01") { SendTime = DateTime.Now });

                    //02  第2秒
                    await Task.Delay(1000);
                    o.OnNext(new CommContext("AB02BB", "AB$1BB", 800, "02") { SendTime = DateTime.Now });

                    //03  第3秒
                    await Task.Delay(1000);
                    o.OnNext(new CommContext("AC03BB", "AC$1BB", 0, "03") { SendTime = DateTime.Now }); //不触发解析

                    //04  第4秒
                    await Task.Delay(1000);
                    o.OnNext(new CommContext("AD04BB", "AD$1BB", 800, "04") { SendTime = DateTime.Now });

                    //05  第5秒
                    await Task.Delay(1000);
                    o.OnNext(new CommContext("AE05BB", "AE$1BB", 800, "05") { SendTime = DateTime.Now });

                    //06  第6秒
                    await Task.Delay(1000);
                    o.OnNext(new CommContext("AF06BB", "AF$1BB", 800, "06") { SendTime = DateTime.Now });

                    o.OnCompleted();
                    return Disposable.Empty;
                })
                .Publish()
            //.RefCount();
            ;

        var obsRecv = Observable.Create<byte[]>(async o =>
                {
                    o.OnNext("AC0000AC".HexToByte());
                    await Task.Delay(500); //500响应时间
                    //01 丢失
                    //第2.5秒 ; 02 接收
                    await Task.Delay(2000);
                    o.OnNext("AB02BB".HexToByte());

                    //第3.5秒 ; 03 不匹配
                    await Task.Delay(1000);
                    o.OnNext("A003BB".HexToByte());

                    //第5.5秒 ; 04 超时 ; 05 接收
                    await Task.Delay(2000);
                    o.OnNext("AD04BB".HexToByte());
                    o.OnNext("AE05BB".HexToByte());

                    //第6.5秒 ; 06接收第一个
                    await Task.Delay(1000);
                    o.OnNext("AF16BB".HexToByte());
                    o.OnNext("AF26BB".HexToByte());

                    o.OnCompleted();
                    return Disposable.Empty;
                })
                .Publish()
            //.RefCount()
            ;

        var cpsr = new List<CommContext>();

        //obsSend.Select(p => p.ToString())
        //    .Merge(obsRecv.Select(p => p.ByteToHex()))
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

        sc.Dispose();
        rc.Dispose();

        Debugger.Break();
    }

    [Fact]
    public async void TimingParsePackageTest()
    {
        var ts = new TestScheduler();
        var ob = ts.CreateObserver<string[]>();

        var a1 = new[] { "a1", "a2", "a3" };
        var a2 = new[] { "b1", "b2", "b3", "b4", "b5", "b6" };
        var a3 = new[] { "c1", "c2", "c3" };

        Observable.Create<string>(async o =>
            {
                foreach (var b in a1) o.OnNext(b);
                await Task.Delay(300);
                foreach (var b in a2)
                {
                    await Task.Delay(50);
                    o.OnNext(b);
                }

                await Task.Delay(300);
                foreach (var b in a3) o.OnNext(b);
                await Task.Delay(300);

                o.OnCompleted();
                return Disposable.Empty;
            })
            .ParsePackage(TimeSpan.FromMilliseconds(100))
            .Subscribe(ob);


        await Task.Delay(1500);

        var rst = ob.Messages
            .Where(p => p.Value.HasValue)
            .Select(p => p.Value.Value)
            .ToList();

        rst.Should().BeEquivalentTo(new List<string[]>
        {
            a1, a2, a3
        });
    }

    [Fact]
    public async void TimeoutBeginEndParsePackageTests()
    {
        // 由于ParsePackageBeginEndTimeoutState中Queue操作与Timer相关,无法使用TestScheduler

        var observable = Observable.Create<string>(async o =>
        {
            foreach (var b in new[] { "0a", "AA", "AA", "01" }) o.OnNext(b);

            await Task.Delay(100);

            foreach (var b in new[] { "BB", "BB", "0b", "AA", "AA", "02", "02", "BB" })
            {
                await Task.Delay(50);
                o.OnNext(b);
            }

            await Task.Delay(300);

            foreach (var b in new[] { "BB", "0c", "AA", "AA", "03", "BB", "BB", "0d" }) o.OnNext(b);

            await Task.Delay(300);

            o.OnCompleted();
            return Disposable.Empty;
        });

        var rst1 = new List<string[]>();
        observable.ParsePackage(new[] { "AA", "AA" }, new[] { "BB", "BB" }, 400)
            .Subscribe(rst1.Add);

        var rst2 = new List<string[]>();
        observable.ParsePackage(new[] { "AA", "AA" }, new[] { "BB", "BB" }, 200)
            .Subscribe(rst2.Add);

        var rst3 = new List<string[]>();
        observable.ParsePackage("AA", "BB", 200, true)
            .Subscribe(rst3.Add);

        await Task.Delay(1200);

        rst1
            .Should()
            .BeEquivalentTo(new List<string[]>
            {
                new[] { "AA", "AA", "01", "BB", "BB" },
                new[] { "AA", "AA", "02", "02", "BB", "BB" },
                new[] { "AA", "AA", "03", "BB", "BB" }
            });

        rst2
            .Should()
            .BeEquivalentTo(new List<string[]>
            {
                new[] { "AA", "AA", "01", "BB", "BB" },
                new[] { "AA", "AA", "03", "BB", "BB" }
            });

        rst3
            .Should()
            .BeEquivalentTo(new List<string[]>
            {
                new[] { "AA", "01", "BB" },
                new[] { "AA", "02", "02", "BB" },
                new[] { "AA", "03", "BB" }
            });
    }

    [Fact]
    public async void TimeoutBeginCountParsePackageTests()
    {
        // 由于ParsePackageBeginCountTimeoutState中Queue操作与Timer相关,无法使用TestScheduler

        var observable = Observable.Create<string>(async o =>
        {
            foreach (var b in new[] { "0a", "AA", "AA", "01" }) o.OnNext(b);

            await Task.Delay(100);

            foreach (var b in new[] { "BB", "BB", "0b", "AA", "AA", "02", "02", "BB" })
            {
                await Task.Delay(50);
                o.OnNext(b);
            }

            await Task.Delay(300);

            foreach (var b in new[] { "BB", "0c", "AA", "AA", "03", "BB", "BB", "0d" }) o.OnNext(b);

            await Task.Delay(300);

            o.OnCompleted();
            return Disposable.Empty;
        });

        var rst1 = new List<string[]>();
        observable.ParsePackage(new[] { "AA", "AA" }, 13, 400)
            .Subscribe(rst1.Add);

        var rst2 = new List<string[]>();
        observable.ParsePackage(new[] { "AA", "AA" }, 13, 200)
            .Subscribe(rst2.Add);

        var rst3 = new List<string[]>();
        observable.ParsePackage("AA", 7, 400)
            .Subscribe(rst3.Add);

        var rst4 = new List<string[]>();
        observable.ParsePackage("AA", 5, 200, true)
            .Subscribe(rst4.Add);

        var rst5 = new List<string[]>();
        observable.ParsePackage("AA", 5, 400, true)
            .Subscribe(rst5.Add);

        await Task.Delay(1200);

        rst1
            .Should()
            .BeEquivalentTo(new List<string[]>
            {
                new[] { "AA", "AA", "01", "BB", "BB", "0b", "AA", "AA", "02", "02", "BB", "BB", "0c" }
            });

        rst2
            .Should()
            .BeEmpty();

        rst3
            .Should()
            .BeEquivalentTo(new List<string[]>
            {
                new[] { "AA", "AA", "01", "BB", "BB", "0b", "AA" },
                new[] { "AA", "AA", "02", "02", "BB", "BB", "0c" }
            });

        rst4
            .Should()
            .BeEquivalentTo(new List<string[]>
            {
                new[] { "AA", "01", "BB", "BB", "0b" },
                new[] { "AA", "03", "BB", "BB", "0d" }
            });

        rst5
            .Should()
            .BeEquivalentTo(new List<string[]>
            {
                new[] { "AA", "01", "BB", "BB", "0b" },
                new[] { "AA", "02", "02", "BB", "BB" },
                new[] { "AA", "03", "BB", "BB", "0d" }
            });
    }
}