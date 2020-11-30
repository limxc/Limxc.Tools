using Xunit;
using Limxc.Tools.DeviceComm.Extensions;
using System;
using System.Collections.Generic;
using System.Text;
using FluentAssertions;

namespace Limxc.Tools.DeviceComm.Extensions.Tests
{
    public class TemplatePraseExtensionTests
    {
        [Fact()]
        public void TemplateLengthTest()
        {
            "AA$123BD$3BB".TemplateLength().Should().Be(2 + 2 + 4 + 6 + 2);
        }

        [Fact()]
        public void IsMatchTest()
        {
            "AA0000$200 00BB".IsMatch("A A0000A2B30000BB").Should().BeTrue();
            "AA0 000$20000BB".IsMatch("AA0000abcd0000B B").Should().BeTrue();
            "AA0000$20000BB".IsMatch("AA0000A2B3D40000BB").Should().BeFalse();
            "AA0000$20000BB".IsMatch("AA0000a10000BB").Should().BeFalse();
            "AA0000$20000BB".IsMatch("AA0000AbcH0000BB").Should().BeFalse();
            "AA0000$20000BB".IsMatch("").Should().BeFalse();
            "AA0000$20000BB".IsMatch(null).Should().BeFalse();
            string s = null;
            s.IsMatch("1").Should().BeFalse();
            s.IsMatch("").Should().BeTrue();

            "2".IsMatch(s).Should().BeFalse();
            "".IsMatch(s).Should().BeTrue(); 
        }
    }
}