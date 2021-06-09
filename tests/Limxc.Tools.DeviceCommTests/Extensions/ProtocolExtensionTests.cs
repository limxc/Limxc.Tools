using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Limxc.Tools.DeviceComm.Extensions;
using Limxc.Tools.DeviceComm.Protocol;
using Limxc.Tools.Entities.Communication;
using Limxc.Tools.Extensions.Communication;
using Xunit;

namespace Limxc.Tools.DeviceCommTests.Extensions
{
    public class ProtocolExtensionTests
    {
        [Fact]
        public async Task SendAsyncTest()
        {
            var simulator = new ProtocolSimulator();

            await simulator.OpenAsync();

            var bytes1 = "AA 01 02 BB".HexToByte();
            var resp1 = await simulator.SendAsync(bytes1, 1000);
            resp1.Should().BeEquivalentTo(bytes1);

            var bytes2 = "CC 01 01 DD".HexToByte();
            var bytes3 = "EE 02 02 FF".HexToByte();
            var resp2 = simulator.SendAsync(bytes2, 1000);
            var resp3 = simulator.SendAsync(bytes3, 1000);

            var res2 = (await resp2).ByteToHex();
            var res3 = (await resp3).ByteToHex();

            //res3应该为 CC 01 01 DD EE 02 02 FF
            //res2为 CC 01 01 DD 或 res3
            res3.Should().Contain(res2);

            await simulator.CloseAsync();
            simulator.Dispose();
        }

        [Fact]
        public async Task WaitingSendResultTest()
        {
            var simulator = new ProtocolSimulator(0, 3000);
            var disposables = new CompositeDisposable();
            var msg = new List<string>();
            var rst = new List<CommContext>();

            simulator.Received.Select(p => $"@ {DateTime.Now:mm:ss fff} 接收 : {p.ByteToHex()}")
                .Subscribe(p => msg.Add(p))
                .DisposeWith(disposables);
            simulator.History.Subscribe(p =>
                {
                    msg.Add($"@ {DateTime.Now:mm:ss fff} {p}");
                    rst.Add(p);
                })
                .DisposeWith(disposables);

            await simulator.OpenAsync();

            //有返回值
            var ctx1 = new CommTaskContext("1", "AA01BB", "AA$1BB", 1000 + 3000);

            var begin = DateTime.Now;
            _ = simulator.SendAsync(ctx1);
            await simulator.WaitingSendResult(ctx1, 1000 + 3000);
            var end = DateTime.Now;

            ctx1.Response.Value.Should().Be("AA01BB");
            (end - begin).TotalMilliseconds.Should().BeApproximately(3500, 500);
            rst.TrueForAll(p => p.State == CommContextState.Success);

            rst.Clear();

            //无返回值
            var ctx2 = new CommTaskContext("2", "000000");
            _ = simulator.SendAsync(ctx2);
            await simulator.WaitingSendResult(ctx2, 3000 + 3000);
            ctx2.State.Should().Be(CommContextState.NoNeed);

            msg.ForEach(p => Debug.WriteLine(p));

            await simulator.CloseAsync();
            simulator.Dispose();
            disposables.Dispose();
        }

        [Fact]
        public async Task ExecQueueTest()
        {
            var simulator = new ProtocolSimulator();
            var disposables = new CompositeDisposable();
            var msg = new List<string>();
            var history = new List<CommTaskContext>();

            simulator.Received.Select(p => $"@ {DateTime.Now:mm:ss fff} 接收 : {p.ByteToHex()}").Subscribe(p =>
                {
                    msg.Add(p);
                })
                .DisposeWith(disposables);
            simulator.History.Subscribe(p =>
            {
                msg.Add($"@ {DateTime.Now:mm:ss fff} {p}");
                history.Add(p as CommTaskContext);
            }).DisposeWith(disposables);

            await simulator.OpenAsync();

            var tcList = new List<CommTaskContext>
            {
                new("id1", "AA01BB", "AA$1BB", 1000, 3),
                new("id2", "AB01BB", "AB$1BB", 1000, 3),
                new("id3", "AC01BB", "AA$1BB", 1000, 3), //失败重试
                new("id4", "AD01BB", 3), //无返回值
                new("id5", "AE01BB", "AE$1BB", 1000, 3)
            };

            await simulator.ExecQueue(tcList, CancellationToken.None, 2000);

            msg.ForEach(p => Debug.WriteLine(p));

            await simulator.CloseAsync();
            simulator.Dispose();
            disposables.Dispose();

            tcList.Count(p => p.State == CommContextState.Timeout).Should().Be(1);
            tcList.Count(p => p.State == CommContextState.NoNeed).Should().Be(1);
            tcList.Count(p => p.State == CommContextState.Success).Should().Be(3);

            history.Count(p => p.State == CommContextState.NoNeed).Should().Be(1);
            history.Count(p => p.State == CommContextState.Success).Should().Be(3);
            history.Count(p => p.State == CommContextState.Timeout).Should().Be(3);
        }
    }
}