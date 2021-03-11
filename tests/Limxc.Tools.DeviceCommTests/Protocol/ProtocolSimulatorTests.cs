using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Force.DeepCloner;
using Limxc.Tools.DeviceComm.Protocol;
using Limxc.Tools.Entities.Communication;
using Limxc.Tools.Extensions.Communication;
using Xunit;

namespace Limxc.Tools.DeviceCommTests.Protocol
{
    public class ProtocolSimulatorTests
    {
        private readonly ProtocolSimulator _simulator;

        public ProtocolSimulatorTests()
        {
            _simulator = new ProtocolSimulator(); //每5秒丢失一个
        }

        [Fact]
        public async Task Test()
        {
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

            _simulator.Received
                .Select(p => $"@ {DateTime.Now:mm:ss fff} 接收 : {p.ToHexStr()}")
                .Subscribe(p => msg.Add(p));

            _simulator.History.Subscribe(p =>
            {
                msg.Add($"@ {DateTime.Now:mm:ss fff} {p}");
                rst.Add(p.DeepClone());
            });

            await _simulator.OpenAsync();

            await _simulator.SendAsync(new CommContext("000000"));

            var loop = 2;
            for (var i = 0; i < loop; i++)
                foreach (var context in sendList)
                    await _simulator.SendAsync(context);

            //await simulator.SendAsync(new CommContext("AA01BB", "AA$1BB", "01") { Timeout = 1000 });
            //await simulator.SendAsync(new CommContext("AB02BB", "AA$1BB", "02") { Timeout = 1000 });//解析失败
            //await simulator.SendAsync(new CommContext("AC03BB", "AA$1BB", "03") { Timeout = 1000 });//解析失败
            //await simulator.SendAsync(new CommContext("AD04BB", "AD$1BB", "04") { Timeout = 1000 });
            //await simulator.SendAsync(new CommContext("AE05BB", "AE$1BB", "05") { Timeout = 1000 });
            //await simulator.SendAsync(new CommContext("AF06BB", "AF$1BB", "06") { Timeout = 1000 });

            //await Task.Delay(1000 * loop);
            await _simulator.CloseAsync();
            _simulator.Dispose();

            msg.Count.Should().Be(2 + 2 * sendList.Count * loop);
            rst.Count(p => p.State == CommContextState.Success).Should().Be(rst.Count - 1);
            rst.Count(p => p.State == CommContextState.NoNeed).Should().Be(1);

            Debugger.Break();
        }
    }
}