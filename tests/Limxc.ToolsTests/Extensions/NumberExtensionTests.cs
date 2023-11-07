using System;
using FluentAssertions;
using Limxc.Tools.Extensions;
using Xunit;

namespace Limxc.ToolsTests.Extensions;

public class NumberExtensionTests
{
    [Fact]
    public void LimitTest()
    {
        double v = 0;
        var a = -5 / v;
        var b = 5 / v;
        var c = 0 / v;

        ((int)a).Limit(0, 50).Should().Be(0);
        ((int)b).Limit(0, 50).Should().Be(0);
        ((int)c).Limit(0, 50).Should().Be(0);
        int.MaxValue.Limit(0, 50).Should().Be(50);
        int.MinValue.Limit(0, 50).Should().Be(0);

        a.Limit(0, 50, 1).Should().Be(0);
        b.Limit(0, 50, 1).Should().Be(50);
        c.Limit(0, 50, 1).Should().Be(0);
        (double.MaxValue + 1000).Limit(0, 50, 1).Should().Be(50);
        (double.MinValue - 1000).Limit(0, 50, 1).Should().Be(0);

        ((float)a).Limit(0, 50).Should().Be(0);
        ((float)b).Limit(0, 50).Should().Be(50);
        ((float)c).Limit(0, 50).Should().Be(0);
        (float.MaxValue + 1000).Limit(0, 50, 1).Should().Be(50);
        (float.MinValue - 1000).Limit(0, 50, 1).Should().Be(0);

        Assert.Throws<OverflowException>(() =>
        {
            ((decimal)a).Limit(0, 50).Should().Be(0);
            ((decimal)b).Limit(0, 50).Should().Be(50);
            ((decimal)c).Limit(0, 50).Should().Be(0);
            decimal.MaxValue.Limit(0, 50, 1).Should().Be(50);
            decimal.MinValue.Limit(0, 50, 1).Should().Be(0);
        });
    }

    [Fact]
    public void MatchTest()
    {
        var ints = new[] { 5, 10, 20 };
        ints.Near(0).Should().Be(5);
        ints.Near(5).Should().Be(5);
        ints.Near(7).Should().Be(5);
        ints.Near(11).Should().Be(10);
        ints.Near(21).Should().Be(20);
        ints.Near(0, false).Should().Be(5);
        ints.Near(5, false).Should().Be(5);
        ints.Near(7, false).Should().Be(10);
        ints.Near(11, false).Should().Be(20);
        ints.Near(21, false).Should().Be(20);

        var doubles = new[] { 5d, 10d, 20d };
        doubles.Near(0).Should().Be(5);
        doubles.Near(5.000).Should().Be(5);
        doubles.Near(7).Should().Be(5);
        doubles.Near(11).Should().Be(10);
        doubles.Near(21).Should().Be(20);
        doubles.Near(0, false).Should().Be(5);
        doubles.Near(5.000).Should().Be(5);
        doubles.Near(7, false).Should().Be(10);
        doubles.Near(11, false).Should().Be(20);
        doubles.Near(21, false).Should().Be(20);

        var floats = new[] { 5f, 10f, 20f };
        floats.Near(0).Should().Be(5);
        floats.Near(5.000f).Should().Be(5);
        floats.Near(7).Should().Be(5);
        floats.Near(11).Should().Be(10);
        floats.Near(21).Should().Be(20);
        floats.Near(0, false).Should().Be(5);
        floats.Near(5.000f).Should().Be(5);
        floats.Near(7, false).Should().Be(10);
        floats.Near(11, false).Should().Be(20);
        floats.Near(21, false).Should().Be(20);

        var decimals = new[] { 5m, 10m, 20m };
        decimals.Near(0).Should().Be(5);
        decimals.Near(5.000m).Should().Be(5);
        decimals.Near(7).Should().Be(5);
        decimals.Near(11).Should().Be(10);
        decimals.Near(21).Should().Be(20);
        decimals.Near(0, false).Should().Be(5);
        decimals.Near(5.000m).Should().Be(5);
        decimals.Near(7, false).Should().Be(10);
        decimals.Near(11, false).Should().Be(20);
        decimals.Near(21, false).Should().Be(20);
    }
}