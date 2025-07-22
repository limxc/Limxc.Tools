using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using FluentAssertions;
using Limxc.Tools.Extensions.Communication;
using Limxc.Tools.SerialPort;
using Xunit;

namespace Limxc.Tools.SerialPortTests;

public class SerialPortServiceSimulatorTests
{
    [Fact]
    public async Task Tests()
    {
        var dis = new CompositeDisposable();
        var dictSendResp = new Dictionary<string, string[]>();
        dictSendResp.Add("AA01BB", ["AA01BB"]);

        var sps = new SerialPortServiceSimulator(0, 0, dictSendResp);

        bool? state = null;
        var received = Array.Empty<byte>();
        sps.ConnectionState.Subscribe(s =>
            {
                state = s;
            })
            .DisposeWith(dis);
        sps.Received.Subscribe(s =>
            {
                received = s;
            })
            .DisposeWith(dis);

        state.Should().BeFalse();
        sps.Start(null);
        await Task.Delay(1200);
        state.Should().BeTrue();

        var hex = "AA 00 BB";
        await sps.SendAsync(hex.HexToByte());
        await Task.Delay(1000);
        received.Should().BeEquivalentTo(Array.Empty<byte>());

        hex = "AA 01 BB";
        var rst = await sps.SendAsync(hex.HexToByte(), 1000);
        rst.Should().BeEquivalentTo(hex.HexToByte());

        var str = await sps.SendAsync(hex, 10000, "AA[2]BB");
        str.Should().BeEquivalentTo("AA01BB");

        dis.Dispose();
    }
}
