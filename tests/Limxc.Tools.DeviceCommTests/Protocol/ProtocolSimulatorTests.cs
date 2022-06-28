using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Force.DeepCloner;
using Limxc.Tools.DeviceComm.Protocol;
using Limxc.Tools.Entities.Communication;
using Limxc.Tools.Extensions.Communication;
using Xunit;

namespace Limxc.Tools.DeviceCommTests.Protocol;

public class ProtocolSimulatorTests
{
    [Fact]
    public async void Test()
    {
        var simulator = new ProtocolSimulator(); //不丢失;
        var disposables = new CompositeDisposable();
        var msg = new List<string>();
        var rst = new List<CommContext>();

        var sendList = new List<CommContext>
        {
            new("AA01BB", "AA$1BB", 1000, "01"),
            new("AB02BB", "AB$1BB", 1000, "02"),
            new("AC03BB", "AC$1BB", 1000, "03"),
            new("AD04BB", "AD$1BB", 1000, "04"),
            new("AE05BB", "AE$1BB", 1000, "05"),
            new("AF06BB", "AF$1BB", 1000, "06")
        };

        simulator.History.Subscribe(p =>
        {
            msg.Add($"History@ {DateTime.Now:mm:ss fff} {p}");
            rst.Add(p.DeepClone());
        }).DisposeWith(disposables);

        simulator.Received
            .Delay(TimeSpan.FromMilliseconds(100)) //美观
            .Select(p => $"Received@ {DateTime.Now:mm:ss fff} 接收 : {p.ByteToHex()}")
            .Subscribe(p => msg.Add(p))
            .DisposeWith(disposables);

        await simulator.OpenAsync();

        await simulator.SendAsync(new CommContext("000000"));

        var loop = 2;
        for (var i = 0; i < loop; i++)
            foreach (var context in sendList)
                await simulator.SendAsync(context);

        await simulator.SendAsync(new CommContext("BB06BB", "CC$1BB", 1000, "06"));

        await Task.Delay(1000);
        await simulator.CloseAsync();
        simulator.Dispose();
        disposables.Dispose();

        msg.Count.Should().Be(2 + 2 * sendList.Count * loop + 2);
        rst.Count(p => p.State == CommContextState.Success).Should().Be(rst.Count - 1 - 1);
        rst.Count(p => p.State == CommContextState.NoNeed).Should().Be(1);
        rst.Count(p => p.State == CommContextState.Timeout).Should().Be(1);
    }

    [Fact]
    public async void LostMsgTest()
    {
        var simulator = new ProtocolSimulator(3); //3丢1;
        var disposables = new CompositeDisposable();
        var msg = new List<string>();
        var rst = new List<CommContext>();
        var sendList = new List<CommContext>
        {
            new("AA01BB", "AA$1BB", 1000, "01"),
            new("AB02BB", "AB$1BB", 1000, "02"),
            new("AC03BB", "AC$1BB", 1000, "03"),
            new("AD04BB", "AD$1BB", 1000, "04"),
            new("AE05BB", "AE$1BB", 1000, "05"),
            new("AF06BB", "AF$1BB", 1000, "06")
        };

        simulator.History.Subscribe(p =>
        {
            msg.Add($"History@ {DateTime.Now:mm:ss fff} {p}");
            rst.Add(p.DeepClone());
        }).DisposeWith(disposables);

        simulator.Received
            .Delay(TimeSpan.FromMilliseconds(100)) //美观
            .Select(p => $"Received@ {DateTime.Now:mm:ss fff} 接收 : {p.ByteToHex()}")
            .Subscribe(p => msg.Add(p))
            .DisposeWith(disposables);

        await simulator.OpenAsync();

        foreach (var context in sendList)
            await simulator.SendAsync(context);

        await Task.Delay(2000);
        await simulator.CloseAsync();
        simulator.Dispose();
        disposables.Dispose();

        msg.Count.Should().Be(2 * sendList.Count - 2);
        rst.Count(p => p.State == CommContextState.Success).Should().Be(rst.Count - 2);
    }
}