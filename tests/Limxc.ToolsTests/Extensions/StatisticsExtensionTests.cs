using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Limxc.Tools.Extensions;
using Xunit;

// ReSharper disable ExpressionIsAlwaysNull
// ReSharper disable PossibleMultipleEnumeration

namespace Limxc.ToolsTests.Extensions;

public class StatisticsExtensionTests
{
    [Fact]
    public void MeanTest()
    {
        IEnumerable<double> source = null;
        source.Mean().Should().Be(0);

        source = Enumerable.Range(1, 100).Select(x => (double)x);
        source.Mean().Should().Be(source.Average());
    }

    [Fact]
    public void MedianTest()
    {
        IEnumerable<double> source = null;
        source.Median().Should().Be(0);

        source = new double[] { 1, 2, 3, 4, 5 };
        source.Median().Should().Be(3);

        source = new double[] { 1, 2, 3, 4, 5, 6 };
        source.Median().Should().Be(3.5);
    }

    [Fact]
    public void ModeTest()
    {
        IEnumerable<double> source = null;
        source.Mode().Should().BeEquivalentTo(Array.Empty<double>());

        source = new double[] { 1, 2, 3, 4 };
        source.Mode().Should().BeEquivalentTo(new double[] { 1, 2, 3, 4 });

        source = new double[] { 1, 2, 3, 3, 4, 5 };
        source.Mode().Should().BeEquivalentTo(new double[] { 3 });

        source = new double[] { 1, 2, 3, 3, 4, 4, 5 };
        source.Mode().Should().BeEquivalentTo(new double[] { 3, 4 });
    }

    [Fact]
    public void VarianceTest()
    {
        IEnumerable<double> source = null;
        source.Variance().Should().Be(0);

        source = new double[] { 1, 2, 3, 4, 5 };
        source.Variance().Should().Be(2);
    }

    [Fact]
    public void SampleVarianceTest()
    {
        IEnumerable<double> source = null;
        source.SampleVariance().Should().Be(0);

        source = new double[] { 1, 2, 3, 4, 5 };
        source.SampleVariance().Should().BeApproximately(2.5, 0.001);
    }

    [Fact]
    public void StandardDeviationTest()
    {
        IEnumerable<double> source = null;
        source.StandardDeviation().Should().Be(0);

        source = new double[] { 1, 2, 3, 4, 5 };
        source.StandardDeviation().Should().BeApproximately(Math.Sqrt(source.Variance()), 0.001);
    }
}
