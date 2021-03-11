using FluentAssertions;
using Xunit;

namespace Limxc.Tools.Extensions.DevComm.Tests
{
    public class TemplatePraseExtensionTests
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
            "AA0000$20000BB".IsTemplateMatch("AA0000A2B3D40000BB").Should().BeFalse();
            "AA0000$20000BB".IsTemplateMatch("AA0000a10000BB").Should().BeFalse();
            "AA0000$20000BB".IsTemplateMatch("AA0000AbcH0000BB").Should().BeFalse();
            "AA0000$20000BB".IsTemplateMatch("").Should().BeFalse();
            "AA0000$20000BB".IsTemplateMatch(null).Should().BeFalse();
            string s = null;
            s.IsTemplateMatch("1").Should().BeFalse();
            s.IsTemplateMatch("").Should().BeTrue();

            "2".IsTemplateMatch(s).Should().BeFalse();
            "".IsTemplateMatch(s).Should().BeTrue();
        }
    }
}