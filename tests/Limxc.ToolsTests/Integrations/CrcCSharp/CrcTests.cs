using System;
using System.Diagnostics;
using System.Text;
using FluentAssertions;
using Limxc.Tools.Extensions.Communication;
using Limxc.Tools.Integrations.CrcCSharp;
using Xunit;

namespace Limxc.ToolsTests.Integrations.CrcCSharp;

public class CrcTests
{
    [Fact]
    public void Test()
    {
        foreach (var p in CrcStdParams.StandartParameters.Values)
        {
            var crc = new Crc(p);
            if (!crc.IsRight())
                Debugger.Break();
        }
    }

    [Fact]
    public void Crc16Test()
    {
        var bytes = Encoding.ASCII.GetBytes("AA0199BB");
        var crc = new Crc(CrcStdParams.StandartParameters[CrcAlgorithms.Crc16Modbus]);

        var hash = crc.ComputeHash(bytes);

        var value = Convert.ToString(BitConverter.ToInt32(hash), 16);

        value.HexFormat().Should().Be("C7 C0");

        value.HexFormat(true).Should().Be("C0 C7");
    }

    [Fact]
    public void Crc32Test()
    {
        var bytes = Encoding.ASCII.GetBytes("AA0199BB");
        var crc = new Crc(CrcStdParams.StandartParameters[CrcAlgorithms.Crc32]);

        var hash = crc.ComputeHash(bytes);

        var value = Convert.ToString(BitConverter.ToInt32(hash), 16);

        value.HexFormat().Should().Be("06 56 91 43");
    }
}
