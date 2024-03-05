using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Limxc.Tools.Extensions;
using Microsoft.Reactive.Testing;
using Xunit;

namespace Limxc.ToolsTests.Extensions;

public class RxExtensionTests
{
    private const long OneSecond = 10000000L;

    /*
        rx不适合处理大量实时数据
     */

    [Fact]
    public async Task BucketTest()
    {
        var ts = new TestScheduler();

        //number bucket
        var ob = ts.CreateObserver<IEnumerable<long>>();
        Observable.Interval(TimeSpan.FromSeconds(1), ts).Take(5).Bucket(2).Subscribe(ob);
        ts.AdvanceBy(5 * OneSecond);
        var rst = ob.Messages.Where(p => p.Value.HasValue).Select(p => p.Value.Value).ToList();
        rst.Should()
            .BeEquivalentTo(
                new List<IEnumerable<long>>
                {
                    new long[] { 0 },
                    new long[] { 0, 1 },
                    new long[] { 1, 2 },
                    new long[] { 2, 3 },
                    new long[] { 3, 4 }
                }
            );

        //time bucket
        var tbList = new List<long[]>();
        Observable
            .Interval(TimeSpan.FromSeconds(0.1))
            .Take(5)
            .Delay(TimeSpan.FromSeconds(0.05))
            .Bucket(TimeSpan.FromSeconds(0.3), TimeSpan.FromSeconds(0.1))
            .Subscribe(p => tbList.Add(p));

        await Task.Delay(1000);

        tbList
            .Should()
            .BeEquivalentTo(
                new List<IEnumerable<long>>
                {
                    new long[] { },
                    new long[] { 0 },
                    new long[] { 0, 1 },
                    new long[] { 0, 1, 2 },
                    new long[] { 1, 2, 3 }
                }
            );

        //time bucket serial
        var tbLists = new List<DateTime[]>();
        Observable
            .Interval(TimeSpan.FromSeconds(0.1))
            .Take(10)
            .Select(p => DateTime.Now)
            .Bucket(TimeSpan.FromSeconds(0.3), TimeSpan.FromSeconds(0.1))
            .Subscribe(p => tbLists.Add(p));

        await Task.Delay(2000);

        foreach (var list in tbLists)
            list.OrderBy(p => p).ToList().SequenceEqual(list).Should().BeTrue();
    }
}
