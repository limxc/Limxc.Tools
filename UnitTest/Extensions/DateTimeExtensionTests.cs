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
            dt.Format().Should().Be("20191101030405");
        }
    }
}