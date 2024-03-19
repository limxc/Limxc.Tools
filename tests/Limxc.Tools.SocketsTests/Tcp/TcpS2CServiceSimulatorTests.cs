using System;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using FluentAssertions;
using Limxc.Tools.Extensions.Communication;
using Limxc.Tools.Sockets.Tcp;
using Xunit;

namespace Limxc.Tools.SocketsTests.Tcp;

public class TcpS2CServiceSimulatorTests
{
    [Fact]
    public async void Tests()
    {
        var dis = new CompositeDisposable();
        ITcpS2CService sim = new TcpS2CServiceSimulator(0, 0);

        bool? state = null;
        var received = Array.Empty<byte>();
        sim.ConnectionState.Subscribe(s => { state = s; })
            .DisposeWith(dis);
        sim.Received.Subscribe(s => { received = s; })
            .DisposeWith(dis);

        state.Should().BeFalse();
        sim.Start(null);
        await Task.Delay(1200);
        state.Should().BeTrue();

        var hex = "AA 01 BB";
        await sim.SendAsync(hex.HexToByte());
        await Task.Delay(1000);
        received.Should().BeEquivalentTo(hex.HexToByte());

        var rst = await sim.SendAsync(hex.HexToByte(), 1000);
        rst.Should().BeEquivalentTo(hex.HexToByte());

        var str = await sim.SendAsync(hex, 1000, "AA[2]BB");
        str.Should().BeEquivalentTo("AA01BB");

        dis.Dispose();
    }
}