using System.Linq;
using System.Net;
using FluentAssertions;
using Limxc.Tools.Sockets.Tcp;
using Xunit;

namespace Limxc.Tools.SocketsTests.Tcp;

public class SerialPortSettingTests
{
    [Fact]
    public void CtorTest()
    {
        var existIpPort =
            Dns.GetHostEntry(Dns.GetHostName())
                .AddressList.FirstOrDefault(p => p.AddressFamily.ToString() == "InterNetwork")
            + ":8080";

        new Setting("", "").Check(out _).Should().BeFalse();
        new Setting("10.9.0.1:8080", "10.9.0.2").Check(out _).Should().BeFalse();
        new Setting(existIpPort, "10.9.0.2").Check(out _).Should().BeTrue();
    }

    private class Setting : TcpS2CSetting
    {
        public Setting(string serverIpPort, string clientIp)
        {
            ServerIpPort = serverIpPort;
            ClientIp = clientIp;
        }
    }
}