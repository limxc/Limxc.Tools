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
        var ts = new TestScheduler();
        var ob = ts.CreateObserver<string[]>();
        var observable = Observable.Create<string>(o =>
        {
            foreach (
                var b in new[]
                {
                    "0a",
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
                    "BB"
                }
            )
                o.OnNext(b);

            o.OnCompleted();
            return Disposable.Empty;
        });

        observable.ParsePackage(new[] { "AA", "AA" }).Subscribe(ob);
        ts.AdvanceTo(100);
        ob.Messages.Where(p => p.Value.HasValue)
            .Select(p => p.Value.Value)
            .ToList()
            .Should()
            .BeEquivalentTo(
                new List<string[]>
                {
                    new[] { "0a", "AA", "AA" },
                    new[] { "01", "BB", "BB", "0b", "AA", "AA" }
                }
            );
        ob.Messages.Clear();

        observable.ParsePackage("AA").Subscribe(ob);
        ts.AdvanceTo(100);
        ob.Messages.Where(p => p.Value.HasValue)
            .Select(p => p.Value.Value)
            .ToList()
            .Should()
            .BeEquivalentTo(
                new List<string[]> { new[] { "0a", "AA" }, new[] { "01", "BB", "BB", "0b", "AA" } }
            );
        ob.Messages.Clear();


        observable.Buffer(2).ParsePackage("AA", "AA").Subscribe(ob);
        ts.AdvanceTo(100);
        ob.Messages.Where(p => p.Value.HasValue)
            .Select(p => p.Value.Value)
            .ToList()
            .Should()
            .BeEquivalentTo(
                new List<string[]>
                {
                    new[] { "0a", "AA", "AA" },
                    new[] { "01", "BB", "BB", "0b", "AA", "AA" }
                }
            );
        ob.Messages.Clear();
    }

    [Fact]
    public void ParsePackagePatternTest()
    {
        var ts = new TestScheduler();
        var ob = ts.CreateObserver<byte[]>();
        var observable = Observable.Create<byte[]>(o =>
        {
            o.OnNext(new byte[] { 1, 2, 1, 2, 0, 0 });
            o.OnNext(new byte[] { 1, 2, 3, 3 });
            o.OnNext(new byte[] { 1, 2, 1, 2, 3, 4 });

            o.OnCompleted();
            return Disposable.Empty;
        });

        var pattern = new byte[] { 1, 2 };

        observable.ParsePackage(pattern).Subscribe(ob);

        ts.AdvanceTo(100);
        ob.Messages.Where(p => p.Value.HasValue)
            .Select(p => p.Value.Value)
            .ToList()
            .Should()
            .BeEquivalentTo(
                new List<byte[]> { new byte[] { 1, 2, 0, 0 }, new byte[] { 1, 2, 3, 3 } }
            );
    }

    [Fact]
    public void BeginEndParsePackageTests()
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

        observable.ParsePackage(new[] { "AA", "AA" }, new[] { "BB", "BB" }).Subscribe(ob);
        ts.AdvanceTo(100);
        ob.Messages.Where(p => p.Value.HasValue)
            .Select(p => p.Value.Value)
            .ToList()
            .Should()
            .BeEquivalentTo(
                new List<string[]>
                {
                    new[] { "AA", "AA", "01", "BB", "BB" },
                    new[] { "AA", "AA", "02", "02", "BB", "BB" },
                    new[] { "AA", "AA", "03", "BB", "BB" }
                }
            );
        ob.Messages.Clear();

        observable.ParsePackage("AA", "BB").Subscribe(ob);
        ts.AdvanceTo(100);
        ob.Messages.Where(p => p.Value.HasValue)
            .Select(p => p.Value.Value)
            .ToList()
            .Should()
            .BeEquivalentTo(
                new List<string[]>
                {
                    new[] { "AA", "AA", "01", "BB" },
                    new[] { "AA", "AA", "02", "02", "BB" },
                    new[] { "AA", "AA", "03", "BB" }
                }
            );
        ob.Messages.Clear();

        observable.ParsePackage("AA", "BB", true).Subscribe(ob);
        ts.AdvanceTo(100);
        ob.Messages.Where(p => p.Value.HasValue)
            .Select(p => p.Value.Value)
            .ToList()
            .Should()
            .BeEquivalentTo(
                new List<string[]>
                {
                    new[] { "AA", "01", "BB" },
                    new[] { "AA", "02", "02", "BB" },
                    new[] { "AA", "03", "BB" }
                }
            );
        ob.Messages.Clear();

        observable.Buffer(2).ParsePackage(new[] { "AA", "AA" }, new[] { "BB", "BB" }).Subscribe(ob);
        ts.AdvanceTo(100);
        ob.Messages.Where(p => p.Value.HasValue)
            .Select(p => p.Value.Value)
            .ToList()
            .Should()
            .BeEquivalentTo(
                new List<string[]>
                {
                    new[] { "AA", "AA", "01", "BB", "BB" },
                    new[] { "AA", "AA", "02", "02", "BB", "BB" },
                    new[] { "AA", "AA", "03", "BB", "BB" }
                }
            );
        ob.Messages.Clear();
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
    public async void TimeoutBeginEndParsePackageTests()
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
    public async void TimeoutBeginCountParsePackageTests()
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

    [Fact]
    public async void TimingParsePackageTest()
    {
        var ts = new TestScheduler();
        var ob = ts.CreateObserver<string[]>();

        var a1 = new[] { "a1", "a2", "a3" };
        var a2 = new[] { "b1", "b2", "b3", "b4", "b5", "b6" };
        var a3 = new[] { "c1", "c2", "c3" };

        Observable
            .Create<string>(async o =>
            {
                foreach (var b in a1)
                    o.OnNext(b);
                await Task.Delay(300);
                foreach (var b in a2)
                {
                    await Task.Delay(50);
                    o.OnNext(b);
                }

                await Task.Delay(300);
                foreach (var b in a3)
                    o.OnNext(b);
                await Task.Delay(300);

                o.OnCompleted();
                return Disposable.Empty;
            })
            .ParsePackage(TimeSpan.FromMilliseconds(100))
            .Subscribe(ob);

        await Task.Delay(1500);

        var rst = ob.Messages.Where(p => p.Value.HasValue).Select(p => p.Value.Value).ToList();

        rst.Should().BeEquivalentTo(new List<string[]> { a1, a2, a3 });
    }
}