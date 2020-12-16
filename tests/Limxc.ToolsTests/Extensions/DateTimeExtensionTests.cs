using FluentAssertions;
using System;
using Xunit;

namespace Limxc.Tools.Extensions.Tests
{
    public class DateTimeExtensionTests
    {
        [Fact()]
        public void Test()
        {
            var dt = new DateTime(2019, 11, 1, 3, 4, 5);
            dt.ToTimeStamp().Should().Be(1572577445000);
            dt.ToTimeStamp().ToDateTime().Should().Be(dt);
        }

        [Fact()]
        public void AgeTest()
        {
            var birth = new DateTime(1999, 8, 11);
            birth.Age(new DateTime(1999, 8, 11)).Should().Be((0, 0, 0));
            birth.Age(new DateTime(1999, 8, 12)).Should().Be((0, 0, 1));
            birth.Age(new DateTime(2020, 11, 5)).Should().Be((21, 2, 25));
        }
    }
}