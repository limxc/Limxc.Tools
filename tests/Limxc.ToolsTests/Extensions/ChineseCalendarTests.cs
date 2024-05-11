using System;
using FluentAssertions;
using Limxc.Tools.Extensions;
using Xunit;

namespace Limxc.ToolsTests.Extensions;

public class ChineseCalendarTests
{
    [Fact]
    public void ChineseCalendarTest()
    {
        var gc = new DateTime(2024, 1, 18, 3, 20, 0);
        var cc = new ChineseCalendar(in gc);
        cc.AnimalString.Should().Be("龙");
        cc.ChineseCalendarHoliday.Should().Be("腊八节");
        cc.GanZhiDateString.Should().Be("癸卯年乙丑月辛巳日");
    }
}