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
            "AA$123BD$3BB".TemplateLength().Should().Be(2+2+4+6+2);
        }
    }
}