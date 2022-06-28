using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Limxc.Tools.DeviceComm.Protocol;
using Xunit;

namespace Limxc.Tools.DeviceCommTests.Protocol;

public class TcpProtocolTests
{
    [Fact]
    public async Task Test()
    {
        var serverAddr = "127.0.0.1:12345";
        var clientAddr = "127.0.0.1:12346 ";

        var server = new TcpServerProtocol();
        var client = new TcpClientProtocol();

        server.Init(serverAddr, clientAddr);
        await server.OpenAsync();

        client.Init(serverAddr);
        await client.OpenAsync();

        var bytes = Encoding.UTF8.GetBytes("testmessage");

        var rec = Array.Empty<byte>();
        var obs = server.Received.Subscribe(b => rec = rec.Concat(b).ToArray());
        var ss = await client.SendAsync(bytes);
        ss.Should().BeTrue();

        await Task.Delay(1000);
        rec.Should().BeEquivalentTo(bytes);

        await server.CloseAsync();
        await client.CloseAsync();
        obs.Dispose();
    }
}