using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Limxc.Tools.Emulator;
using Limxc.Tools.Extensions;
using Limxc.Tools.Extensions.Communication;
using Limxc.Tools.SerialPort;
using Xunit;

namespace Limxc.Tools.EmulatorTests;

public class VirtualComTests
{
    [Fact]
    public async Task TestAsyncTest()
    {
        var disposables = new CompositeDisposable();
        VirtualCom vc = null;
        try
        {
            vc = new VirtualCom(Environment.CurrentDirectory);
        }
        catch
        {
            // ignored
        }

        if (vc == null) return;

        var comStart = 250;
        var baudRate = 9600;

        var dis = await vc.TestAsync(baudRate, comStart);

        var com1 = $"COM{comStart}";
        var com2 = $"COM{comStart + 3}";

        var sps1Recvs = new List<string>();
        var sps2Recvs = new List<string>();

        var sps1 = new SerialPortService();
        sps1.Start(new SerialPortSetting
        {
            BaudRate = baudRate,
            PortName = com1
        });

        var sps2 = new SerialPortService();
        sps2.Start(new SerialPortSetting
        {
            BaudRate = baudRate,
            PortName = com2
        });

        sps1.Received.Select(p => p.ByteToHex()).Subscribe(sps1Recvs.Add).DisposeWith(disposables);
        sps2.Received.Select(p => p.ByteToHex()).Subscribe(sps2Recvs.Add).DisposeWith(disposables);

        sps1.ConnectionState.CombineLatest(sps2.ConnectionState)
            .Where(p => p is { First: true, Second: true })
            .Take(5)
            .CallAsync(async _ =>
            {
                await sps1.SendAsync("AA".HexToByte());
                await sps2.SendAsync("BB".HexToByte());
            })
            .Subscribe()
            .DisposeWith(disposables);


        await Task.Delay(5000);

        sps1Recvs.FindAll(p => p == "BB").Count.Should().BeGreaterThanOrEqualTo(1);
        sps2Recvs.FindAll(p => p == "AA").Count.Should().BeGreaterThanOrEqualTo(1);

        disposables.Dispose();
        dis.Dispose();
    }
}