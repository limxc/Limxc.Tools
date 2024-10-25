using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Limxc.Tools.Extensions.Communication;
using Microsoft.Reactive.Testing;
using Xunit;

namespace Limxc.ToolsTests.Extensions.Communication;

public class ParseExtensionTests
{ 
    [Fact]
    public void ParsePackageSeparatorTest()
    {
        var obs = new byte[]
        {
            1, 2, 3, 4, 5,
            1, 2, 3, 4, 5, 6, 7, 8, 9,
            1, 2, 3, 4, 5, 6, 7
        }.ToObservable();

        byte sep1 = 3;
        var exp1 = new List<byte[]>
        {
            new byte[] { 1, 2 },
            new byte[] { 4, 5, 1, 2 },
            new byte[] { 4, 5, 6, 7, 8, 9, 1, 2 },
            new byte[] { 4, 5, 6, 7 }
        };

        var sep2 = new byte[] { 1, 2 };
        var exp2 = new List<byte[]>
        {
            new byte[] { 3, 4, 5 },
            new byte[] { 3, 4, 5, 6, 7, 8, 9 },
            new byte[] { 3, 4, 5, 6, 7 },
        };

        List<byte[]> rst = null;

        var ts = new TestScheduler();
        var ob = ts.CreateObserver<byte[]>();

        obs.ParsePackage(sep1).Subscribe(ob);
        ts.AdvanceTo(100);
        rst = ob.Messages.Where(p => p.Value.HasValue)
            .Select(p => p.Value.Value)
            .ToList();
        rst.Should().BeEquivalentTo(exp1);
        ob.Messages.Clear();

        obs.ParsePackage(sep2).Subscribe(ob);
        ts.AdvanceTo(100);
        rst = ob.Messages.Where(p => p.Value.HasValue)
            .Select(p => p.Value.Value)
            .ToList();
        rst.Should().BeEquivalentTo(exp2);
        ob.Messages.Clear();

        obs.Buffer(3).ParsePackage(sep2).Subscribe(ob);
        ts.AdvanceTo(100);
        rst = ob.Messages.Where(p => p.Value.HasValue)
            .Select(p => p.Value.Value)
            .ToList();
        rst.Should().BeEquivalentTo(exp2);
        ob.Messages.Clear();
    }

    [Fact]
    public void BeginEndParsePackageTests()
    {
        var obs = new byte[]
        {
            1, 2, 2, 3, 4, 5, 5,
            1, 2, 3, 2, 3, 4, 5, 6, 5, 6, 7, 8, 9,
            1, 2, 3, 4, 5, 6, 7,
            2, 3
        }.ToObservable();

        byte sep1Bom = 2;
        byte sep1Eom = 5;
        var exp1 = new List<byte[]>
        {
            new byte[] { 2, 2, 3, 4, 5 },
            new byte[] { 2, 3, 2, 3, 4, 5 },
            new byte[] { 2, 3, 4, 5 }
        };

        var sep2Bom = new byte[] { 2, 3 };
        var sep2Eom = new byte[] { 5, 6 };
        var exp2 = new List<byte[]>
        {
            new byte[] { 2, 3, 4, 5, 5, 1, 2, 3, 2, 3, 4, 5, 6 },
            new byte[] { 2, 3, 4, 5, 6 }
        };

        List<byte[]> rst = null;
        var ts = new TestScheduler();
        var ob = ts.CreateObserver<byte[]>();

        obs.ParsePackage(sep1Bom, sep1Eom).Subscribe(ob);
        ts.AdvanceTo(100);
        rst = ob.Messages.Where(p => p.Value.HasValue)
            .Select(p => p.Value.Value)
            .ToList();
        rst.Should().BeEquivalentTo(exp1);
        ob.Messages.Clear();

        obs.ParsePackage(sep2Bom, sep2Eom).Subscribe(ob);
        ts.AdvanceTo(100);
        rst = ob.Messages.Where(p => p.Value.HasValue)
            .Select(p => p.Value.Value)
            .ToList();
        rst.Should().BeEquivalentTo(exp2);
        ob.Messages.Clear();

        obs.Buffer(3).ParsePackage(sep2Bom, sep2Eom).Subscribe(ob);
        ts.AdvanceTo(100);
        rst = ob.Messages.Where(p => p.Value.HasValue)
            .Select(p => p.Value.Value)
            .ToList();
        rst.Should().BeEquivalentTo(exp2);
        ob.Messages.Clear();

        // useLastBom

        byte sep1BomL = 2;
        byte sep1EomL = 5;
        var exp1L = new List<byte[]>
        {
            new byte[] { 2, 3, 4, 5 },
            new byte[] { 2, 3, 4, 5 },
            new byte[] { 2, 3, 4, 5 }
        };

        var sep2BomL = new byte[] { 2, 3 };
        var sep2EomL = new byte[] { 5, 6 };
        var exp2L = new List<byte[]>
        {
            new byte[] { 2, 3, 4, 5, 6 },
            new byte[] { 2, 3, 4, 5, 6 }
        };

        obs.ParsePackage(sep1BomL, sep1EomL, true).Subscribe(ob);
        ts.AdvanceTo(100);
        rst = ob.Messages.Where(p => p.Value.HasValue)
            .Select(p => p.Value.Value)
            .ToList();
        rst.Should().BeEquivalentTo(exp1L);
        ob.Messages.Clear();

        obs.ParsePackage(sep2BomL, sep2EomL, true).Subscribe(ob);
        ts.AdvanceTo(100);
        rst = ob.Messages.Where(p => p.Value.HasValue)
            .Select(p => p.Value.Value)
            .ToList();
        rst.Should().BeEquivalentTo(exp2L);
        ob.Messages.Clear();

        obs.Buffer(3).ParsePackage(sep2BomL, sep2EomL, true).Subscribe(ob);
        ts.AdvanceTo(100);
        rst = ob.Messages.Where(p => p.Value.HasValue)
            .Select(p => p.Value.Value)
            .ToList();
        rst.Should().BeEquivalentTo(exp2L);
        ob.Messages.Clear();
    }

    [Fact]
    public async void BeginEndTimeoutParsePackageTests()
    {
        // 由于ParsePackageBeginEndTimeoutState中Queue操作与Timer相关,无法使用TestScheduler

        var observable = Observable.Create<string>(async o =>
        {
            foreach (var b in new[] { "0a", "AA", "AA", "01" })
                o.OnNext(b);

            await Task.Delay(100);

            foreach (var b in new[] { "BB", "BB", "0b", "AA", "AA", "02", "02", "BB" })
            {
                await Task.Delay(50);
                o.OnNext(b);
            }

            await Task.Delay(300);

            foreach (var b in new[] { "BB", "0c", "AA", "AA", "03", "BB", "BB", "0d" })
                o.OnNext(b);

            await Task.Delay(300);

            o.OnCompleted();
            return Disposable.Empty;
        });

        var rst1 = new List<string[]>();
        observable
            .ParsePackage(new[] { "AA", "AA" }, new[] { "BB", "BB" }, 400)
            .Subscribe(rst1.Add);

        var rst2 = new List<string[]>();
        observable
            .ParsePackage(new[] { "AA", "AA" }, new[] { "BB", "BB" }, 200)
            .Subscribe(rst2.Add);

        var rst3 = new List<string[]>();
        observable.ParsePackage("AA", "BB", 200, true).Subscribe(rst3.Add);

        await Task.Delay(1200);

        rst1.Should()
            .BeEquivalentTo(
                new List<string[]>
                {
                    new[] { "AA", "AA", "01", "BB", "BB" },
                    new[] { "AA", "AA", "02", "02", "BB", "BB" },
                    new[] { "AA", "AA", "03", "BB", "BB" }
                }
            );

        rst2.Should()
            .BeEquivalentTo(
                new List<string[]>
                {
                    new[] { "AA", "AA", "01", "BB", "BB" },
                    new[] { "AA", "AA", "03", "BB", "BB" }
                }
            );

        rst3.Should()
            .BeEquivalentTo(
                new List<string[]>
                {
                    new[] { "AA", "01", "BB" },
                    new[] { "AA", "02", "02", "BB" },
                    new[] { "AA", "03", "BB" }
                }
            );
    }

    [Fact]
    public void BeginCountParsePackageTests()
    {
        var ts = new TestScheduler();
        var ob = ts.CreateObserver<string[]>();
        var observable = Observable.Create<string>(o =>
        {
            foreach (var b in new[] { "0a", "AA", "AA", "01" })
                o.OnNext(b);

            foreach (var b in new[] { "BB", "BB", "0b", "AA", "AA", "02", "02", "BB" })
                o.OnNext(b);

            foreach (var b in new[] { "BB", "0c", "AA", "AA", "03", "BB", "BB", "0d" })
                o.OnNext(b);

            o.OnCompleted();
            return Disposable.Empty;
        });

        observable.ParsePackage(new[] { "AA", "AA" }, 4).Subscribe(ob);
        ts.AdvanceTo(100);
        ob.Messages.Where(p => p.Value.HasValue)
            .Select(p => p.Value.Value)
            .ToList()
            .Should()
            .BeEquivalentTo(
                new List<string[]>
                {
                    new[] { "AA", "AA", "01", "BB" },
                    new[] { "AA", "AA", "02", "02" },
                    new[] { "AA", "AA", "03", "BB" }
                }
            );
        ob.Messages.Clear();

        observable.ParsePackage("AA", 5).Subscribe(ob);
        ts.AdvanceTo(100);
        ob.Messages.Where(p => p.Value.HasValue)
            .Select(p => p.Value.Value)
            .ToList()
            .Should()
            .BeEquivalentTo(
                new List<string[]>
                {
                    new[] { "AA", "AA", "01", "BB", "BB" },
                    new[] { "AA", "AA", "02", "02", "BB" },
                    new[] { "AA", "AA", "03", "BB", "BB" }
                }
            );
        ob.Messages.Clear();

        observable.ParsePackage("AA", 5, true).Subscribe(ob);
        ts.AdvanceTo(100);
        ob.Messages.Where(p => p.Value.HasValue)
            .Select(p => p.Value.Value)
            .ToList()
            .Should()
            .BeEquivalentTo(
                new List<string[]>
                {
                    new[] { "AA", "01", "BB", "BB", "0b" },
                    new[] { "AA", "02", "02", "BB", "BB" },
                    new[] { "AA", "03", "BB", "BB", "0d" }
                }
            );

        ob.Messages.Clear();
        observable.Buffer(2).ParsePackage(new[] { "AA", "AA" }, 4).Subscribe(ob);
        ts.AdvanceTo(100);
        ob.Messages.Where(p => p.Value.HasValue)
            .Select(p => p.Value.Value)
            .ToList()
            .Should()
            .BeEquivalentTo(
                new List<string[]>
                {
                    new[] { "AA", "AA", "01", "BB" },
                    new[] { "AA", "AA", "02", "02" },
                    new[] { "AA", "AA", "03", "BB" }
                }
            );
        ob.Messages.Clear();
    }

    [Fact]
    public async void BeginCountTimeoutParsePackageTests()
    {
        // 由于ParsePackageBeginCountTimeoutState中Queue操作与Timer相关,无法使用TestScheduler

        var observable = Observable.Create<string>(async o =>
        {
            foreach (var b in new[] { "0a", "AA", "AA", "01" })
                o.OnNext(b);

            await Task.Delay(100);

            foreach (var b in new[] { "BB", "BB", "0b", "AA", "AA", "02", "02", "BB" })
            {
                await Task.Delay(50);
                o.OnNext(b);
            }

            await Task.Delay(300);

            foreach (var b in new[] { "BB", "0c", "AA", "AA", "03", "BB", "BB", "0d" })
                o.OnNext(b);

            await Task.Delay(300);

            o.OnCompleted();
            return Disposable.Empty;
        });

        var rst1 = new List<string[]>();
        observable.ParsePackage(new[] { "AA", "AA" }, 13, 400).Subscribe(rst1.Add);

        var rst2 = new List<string[]>();
        observable.ParsePackage(new[] { "AA", "AA" }, 13, 200).Subscribe(rst2.Add);

        var rst3 = new List<string[]>();
        observable.ParsePackage("AA", 7, 400).Subscribe(rst3.Add);

        var rst4 = new List<string[]>();
        observable.ParsePackage("AA", 5, 200, true).Subscribe(rst4.Add);

        var rst5 = new List<string[]>();
        observable.ParsePackage("AA", 5, 400, true).Subscribe(rst5.Add);

        await Task.Delay(1200);

        rst1.Should()
            .BeEquivalentTo(
                new List<string[]>
                {
                    new[]
                    {
                        "AA",
                        "AA",
                        "01",
                        "BB",
                        "BB",
                        "0b",
                        "AA",
                        "AA",
                        "02",
                        "02",
                        "BB",
                        "BB",
                        "0c"
                    }
                }
            );

        rst2.Should().BeEmpty();

        rst3.Should()
            .BeEquivalentTo(
                new List<string[]>
                {
                    new[] { "AA", "AA", "01", "BB", "BB", "0b", "AA" },
                    new[] { "AA", "AA", "02", "02", "BB", "BB", "0c" }
                }
            );

        rst4.Should()
            .BeEquivalentTo(
                new List<string[]>
                {
                    new[] { "AA", "01", "BB", "BB", "0b" },
                    new[] { "AA", "03", "BB", "BB", "0d" }
                }
            );

        rst5.Should()
            .BeEquivalentTo(
                new List<string[]>
                {
                    new[] { "AA", "01", "BB", "BB", "0b" },
                    new[] { "AA", "02", "02", "BB", "BB" },
                    new[] { "AA", "03", "BB", "BB", "0d" }
                }
            );
    }
}