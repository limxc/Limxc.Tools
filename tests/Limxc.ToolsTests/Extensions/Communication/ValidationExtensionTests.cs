using System.Linq;
using FluentAssertions;
using Limxc.Tools.Extensions.Communication;
using Xunit;

namespace Limxc.ToolsTests.Extensions.Communication;

public class ValidationExtensionTests
{
    [Fact]
    public void ChecksumTest()
    {
        var data = "DB 03 01 46".HexToByte().ToArray();
        byte cs = 0x25;
        data.Checksum().Should().Be(cs);
    }
}