using Xunit;
using Limxc.Tools.Extensions;
using System;
using System.Collections.Generic;
using System.Text;
using FluentAssertions;

namespace Limxc.Tools.Extensions.Tests
{
    public class ValidatorExtensionTests
    {
        [Fact()]
        public void CheckIpPortTest()
        {
            "0.0.0.0:0".CheckIpPort().Should().BeTrue();
            "10.20.30.40:50".CheckIpPort().Should().BeTrue();
            "255.255.255.255:65535".CheckIpPort().Should().BeTrue();
            
            "-1.0.0.0:0".CheckIpPort().Should().BeFalse();
            "255.255.255.255:-1".CheckIpPort().Should().BeFalse();
            "255.255.255.255:65536".CheckIpPort().Should().BeFalse();
            "256.255.255.255:65535".CheckIpPort().Should().BeFalse();
              
            "255.255.255.255: 65535".CheckIpPort().Should().BeFalse();
            "255.255 .255.255:65535".CheckIpPort().Should().BeFalse(); 
        }
    }
}