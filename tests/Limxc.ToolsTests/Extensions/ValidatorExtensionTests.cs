using FluentAssertions;
using Limxc.Tools.Extensions;
using Xunit;

namespace Limxc.ToolsTests.Extensions
{
    public class ValidatorExtensionTests
    {
        [Fact]
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

        [Fact]
        public void CheckIpTest()
        {
            "0.0.0.0".CheckIp().Should().BeTrue();
            "10.20.30.40".CheckIp().Should().BeTrue();
            "255.255.255.255".CheckIp().Should().BeTrue();

            "-1.0.0.0".CheckIp().Should().BeFalse();
            "256.254.255.255".CheckIp().Should().BeFalse();
            "255.256.255.255".CheckIp().Should().BeFalse();
            "255.255.256.255".CheckIp().Should().BeFalse();
            "255.255.256.256".CheckIp().Should().BeFalse();

            "255.255 .255.255".CheckIp().Should().BeFalse();
        }

        [Fact]
        public void CheckPortTest()
        {
            "-1".CheckPort().Should().BeFalse();
            "65536".CheckPort().Should().BeFalse();

            "4".CheckPort().Should().BeTrue();
            "25".CheckPort().Should().BeTrue();
            "254".CheckPort().Should().BeTrue();
            "6625".CheckPort().Should().BeTrue();
        }

        [Fact]
        public void CheckEmailTest()
        {
            "xxx@asd.com".CheckEmail().Should().BeTrue();

            " xxx@asd.com ".CheckEmail().Should().BeFalse();

            "xxx.asd.com".CheckEmail().Should().BeFalse();
            "xxx.asd@com".CheckEmail().Should().BeFalse();
            "xxxasd.com".CheckEmail().Should().BeFalse();
        }
    }
}