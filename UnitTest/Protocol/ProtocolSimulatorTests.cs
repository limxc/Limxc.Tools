using FluentAssertions;
using Limxc.Tools.DeviceComm.Entities;
using Limxc.Tools.DeviceComm.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Threading.Tasks;
using UnitTest.TestUtils;
using Xunit;

namespace UnitTest.Protocol
{
    public class ProtocolSimulatorTests
    {
        private ProtocolSimulator simulator;

        public ProtocolSimulatorTests()
        {
            simulator = new ProtocolSimulator();//每5秒丢失一个
        }

        [Fact]
        public async Task Test()
        {
            var msg = new List<string>();
            var rst = new List<CPContext>();

            simulator.ConnectionState.Select(p => $"@ {DateTime.Now:mm:ss fff} 连接状态 : {p}").Subscribe(p => msg.Add(p));
            simulator.Received.Select(p => $"@ {DateTime.Now:mm:ss fff} 接收 : {p.ToHexStr()}").Subscribe(p => msg.Add(p));
            simulator.History.Subscribe(p =>
            {
                msg.Add($"@ {DateTime.Now:mm:ss fff} {p}");
                rst.Add(p);
            });

            await simulator.OpenAsync();

            int loop = 5;
            for (int i = 0; i < loop; i++)
            {
                await simulator.SendAsync(new CPContext("AA01BB", "AA$1BB", 1000, "01"));
                await simulator.SendAsync(new CPContext("AB02BB", "AB$1BB", 1000, "02"));
                await simulator.SendAsync(new CPContext("AC03BB", "AC$1BB", 1000, "03"));
                await simulator.SendAsync(new CPContext("AD04BB", "AD$1BB", 1000, "04"));
                await simulator.SendAsync(new CPContext("AE05BB", "AE$1BB", 1000, "05"));
                await simulator.SendAsync(new CPContext("AF06BB", "AF$1BB", 1000, "06"));
            }

            //await simulator.SendAsync(new CPContext("AA01BB", "AA$1BB", "01") { Timeout = 1000 });
            //await simulator.SendAsync(new CPContext("AB02BB", "AA$1BB", "02") { Timeout = 1000 });//解析失败
            //await simulator.SendAsync(new CPContext("AC03BB", "AA$1BB", "03") { Timeout = 1000 });//解析失败
            //await simulator.SendAsync(new CPContext("AD04BB", "AD$1BB", "04") { Timeout = 1000 });
            //await simulator.SendAsync(new CPContext("AE05BB", "AE$1BB", "05") { Timeout = 1000 });
            //await simulator.SendAsync(new CPContext("AF06BB", "AF$1BB", "06") { Timeout = 1000 });

            await simulator.CloseAsync();
            simulator.CleanUp();

            msg.Count.Should().Be(2 + 2 * 6 * loop);
            rst.TrueForAll(p => p.Status == CPContextStatus.Success);

            Debugger.Break();
        }
    }
}