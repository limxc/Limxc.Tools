using System;
using FluentAssertions;
using Limxc.Tools.Extensions;
using Xunit;

namespace Limxc.ToolsTests.Extensions
{
    public class NumberExtensionTests
    {
        [Fact]
        public void LimitTest()
        {
            double v = 0;
            var a = -5 / v;
            var b = 5 / v;
            var c = 0 / v;

            ((int) a).Limit(0, 50).Should().Be(0);
            ((int) b).Limit(0, 50).Should().Be(0);
            ((int) c).Limit(0, 50).Should().Be(0);
            int.MaxValue.Limit(0, 50).Should().Be(50);
            int.MinValue.Limit(0, 50).Should().Be(0);

            a.Limit(0, 50, 1).Should().Be(0);
            b.Limit(0, 50, 1).Should().Be(50);
            c.Limit(0, 50, 1).Should().Be(0);
            (double.MaxValue + 1000).Limit(0, 50, 1).Should().Be(50);
            (double.MinValue - 1000).Limit(0, 50, 1).Should().Be(0);
            
            ((float) a).Limit(0, 50).Should().Be(0);
            ((float) b).Limit(0, 50).Should().Be(50);
            ((float) c).Limit(0, 50).Should().Be(0);
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
    }
}