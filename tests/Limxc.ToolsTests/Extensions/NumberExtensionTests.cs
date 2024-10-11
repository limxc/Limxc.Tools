using System;
using System.Collections.Generic;
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
        ints.Nearest(7).Should().Be(5);
        ints.Nearest(8).Should().Be(10);


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
        doubles.Nearest(7).Should().Be(5);
        doubles.Nearest(7.5).Should().Be(5);
        doubles.Nearest(8).Should().Be(10);

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
        floats.Nearest(7).Should().Be(5);
        floats.Nearest(7.5f).Should().Be(5);
        floats.Nearest(8).Should().Be(10);

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
        decimals.Nearest(7).Should().Be(5);
        decimals.Nearest(7.5m).Should().Be(5);
        decimals.Nearest(8).Should().Be(10);

        var intObjs = new[] { ("A", 5), ("B", 10), ("C", 20) };
        intObjs.NearBy(p => p.Item2, 6).Item1.Should().Be("A");
        intObjs.NearBy(p => p.Item2, 6, false).Item1.Should().Be("B");
        intObjs.NearestBy(p => p.Item2, 6).Item1.Should().Be("A");

        var doubleObjs = new[] { new { K = "A", V = 5d }, new { K = "B", V = 10d }, new { K = "C", V = 20d } };
        doubleObjs.NearBy(p => p.V, 6).K.Should().Be("A");
        doubleObjs.NearBy(p => p.V, 6, false).K.Should().Be("B");
        doubleObjs.NearestBy(p => p.V, 6).K.Should().Be("A");

        var floatObjs = new Dictionary<string, float>
        {
            { "A", 5f },
            { "B", 10f },
            { "C", 20f }
        };
        floatObjs.NearBy(p => p.Value, 6).Key.Should().Be("A");
        floatObjs.NearBy(p => p.Value, 6, false).Key.Should().Be("B");
        floatObjs.NearestBy(p => p.Value, 6).Key.Should().Be("A");

        var decimalObjs = new[]
        {
            new DecimalObj("A", 5m),
            new DecimalObj("B", 10m),
            new DecimalObj("C", 20m)
        };
        decimalObjs.NearBy(p => p.Value, 6).Key.Should().Be("A");
        decimalObjs.NearBy(p => p.Value, 6, false).Key.Should().Be("B");
        decimalObjs.NearestBy(p => p.Value, 6).Key.Should().Be("A");
    }

    private class DecimalObj
    {
        public DecimalObj()
        {
        }

        public DecimalObj(string key, decimal value)
        {
            Key = key;
            Value = value;
        }

        public string Key { get; }
        public decimal Value { get; }
    }
}