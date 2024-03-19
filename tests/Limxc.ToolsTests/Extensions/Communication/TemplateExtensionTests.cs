using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Limxc.Tools.Extensions.Communication;
using Xunit;

namespace Limxc.ToolsTests.Extensions.Communication;

public class TemplateExtensionTests
{
    [Fact]
    public void GetLengthByTemplateTest()
    {
        "AA [4] AB [2] [8] BB".GetLengthByTemplate().Should().Be(2 + 2 * 2 + 2 + 1 * 2 + 4 * 2 + 2);
    }

    [Fact]
    public void SimulateByTemplateTest()
    {
        var template = "AA00[4][2]BB";
        template.SimulateByTemplate().IsTemplateMatch(template).Should().BeTrue();

        "".SimulateByTemplate().Should().BeEmpty();
    }

    [Fact]
    public void IsTemplateMatchTest()
    {
        "AA02BB".IsTemplateMatch("AA[2]BB").Should().BeTrue();
        "A A0000A2B30000BB".IsTemplateMatch("AA0000[4]00 00BB").Should().BeTrue();
        "AA0000abcd0000B B".IsTemplateMatch("AA0 000[4]0000BB").Should().BeTrue();
    }

    [Fact]
    public void TryGetTemplateMatchResultsTest()
    {
        var template = "AA00[4][2]BB";
        var resp = "AA00 0805 44BB";
        var respT = resp.Replace(" ", "");
        "A1A00 0805 44BB".TryGetTemplateMatchResults(template).Should().BeNullOrEmpty();
        resp.TryGetTemplateMatchResults(template).FirstOrDefault().Should().Be(respT);
        (resp + "123").TryGetTemplateMatchResults(template).FirstOrDefault().Should().Be(respT);
        ("123" + resp).TryGetTemplateMatchResults(template).FirstOrDefault().Should().Be(respT);
        ("123" + resp + "123")
            .TryGetTemplateMatchResults(template)
            .FirstOrDefault()
            .Should()
            .Be(respT);
        resp.TryGetTemplateMatchResults(template).FirstOrDefault().Should().Be(respT);
        resp.TryGetTemplateMatchResults(template).FirstOrDefault().Should().Be(respT);
        (resp + resp)
            .TryGetTemplateMatchResults(template)
            .Should()
            .BeEquivalentTo(new List<string> { respT, respT });
    }

    [Fact]
    public void GetMatchValuesTest()
    {
        var template = "AA[2][4]BB";
        "AA010203BB".GetMatchValues(template).Should().BeEquivalentTo("01", "0203");

        var act = () => "0000".GetMatchValues(template);
        act.Should().Throw<Exception>();
    }
}