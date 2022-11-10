using System;
using FluentAssertions;
using Limxc.Tools.Extensions.Communication;
using Xunit;

namespace Limxc.ToolsTests.Extensions.Communication;

public class CommContextExtensionTests
{
    [Fact]
    public void TemplateLengthTest()
    {
        "AA$123BD$3BB".TemplateLength().Should().Be(2 + 2 + 4 + 6 + 2);
    }

    [Fact]
    public void IsMatchTest()
    {
        "AA$1BB".IsTemplateMatch("AA02BB").Should().BeTrue();
        "AA0000$200 00BB".IsTemplateMatch("A A0000A2B30000BB").Should().BeTrue();
        "AA0 000$20000BB".IsTemplateMatch("AA0000abcd0000B B").Should().BeTrue();

        "AA0 000$20000BB".IsTemplateMatch("AA0000abcd0000B").Should().BeFalse();
        "AA0 000$20000BB".IsTemplateMatch("12AA0000abcd0000B B").Should().BeFalse();
        "AA0 000$20000BB".IsTemplateMatch("AA0000abcd0000B B12").Should().BeFalse();
        "AA0 000$20000BB".IsTemplateMatch("12AA0000abcd0000B B12").Should().BeFalse();

        "AA0000$20000BB".IsTemplateMatch("AA0000A2B3D40000BB").Should().BeFalse();
        "AA0000$20000BB".IsTemplateMatch("AA0000a10000BB").Should().BeFalse();
        "AA0000$20000BB".IsTemplateMatch("AA0000AbcH0000BB").Should().BeFalse();

        "AA0000$20000BB".IsTemplateMatch("").Should().BeFalse();
        "AA0000$20000BB".IsTemplateMatch(null).Should().BeFalse();

        "AA0000220000BB".IsTemplateMatch("AA0000 220000BB").Should().BeTrue();
        "AA0000220000BB".IsTemplateMatch("AA0000 20000BB").Should().BeFalse();
        "AA0000220000BB".IsTemplateMatch("AA0000220000BB123").Should().BeFalse();
        "AA0000220000BB".IsTemplateMatch("123AA0000220000BB").Should().BeFalse();
        "AA0000220000BB".IsTemplateMatch("123AA0000220000BB123").Should().BeFalse();

        var s = string.Empty;
        s.IsTemplateMatch("1").Should().BeFalse();
        s.IsTemplateMatch("").Should().BeTrue();

        "2".IsTemplateMatch(s).Should().BeFalse();
        "".IsTemplateMatch(s).Should().BeTrue();
    }

    [Fact]
    public void TryGetTemplateMatchResultTest()
    {
        var template = "AA00$2$1BB";
        var resp = "AA00 0805 44BB".Replace(" ", "");
        template.TryGetTemplateMatchResult("A1A00 0805 44BB").Should().BeNullOrEmpty();
        template.TryGetTemplateMatchResult(resp).Should().Be(resp);
        template.TryGetTemplateMatchResult(resp + "123").Should().Be(resp);
        template.TryGetTemplateMatchResult("123" + resp).Should().Be(resp);
        template.TryGetTemplateMatchResult("123" + resp + "123").Should().Be(resp);
    }

    [Fact]
    public void SimulateResponseTest()
    {
        var template = "AA00$2$1BB";
        var resp = template.SimulateResponse();
        template.IsTemplateMatch(resp).Should().BeTrue();

        "".SimulateResponse().Should().BeEmpty();
    }

    [Fact]
    public void GetValuesTest()
    {
        var template = "AA$1$2BB";
        "AA010203BB".GetValues(template).Should().BeEquivalentTo("01", "0203");
        var act = () => "0000".GetValues(template);
        act.Should().Throw<FormatException>();
    }
}