﻿using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Limxc.Tools.DeviceComm.Protocol;
using Limxc.Tools.Entities.Communication;
using Limxc.Tools.Extensions.Communication;
using Xunit;

namespace Limxc.Tools.DeviceCommTests.Protocol
{
    public class SerialPortProtocolTests
    {
        [Fact]
        public async Task Test()
        {
            /*
             * 需要硬件环境
             * rs232连接2-3, 自发自回
             */
            //var r = new Random(Guid.NewGuid().GetHashCode());
            //if (r.NextDouble() > 0.5)
            //    await ManualTest();
            //else
            //    await AutoTest();

            //await ManualTest();
            await AutoTest();
        }

        private async Task ManualTest()
        {
            /*
             * 需要硬件环境
             * rs232连接2-3, 自发自回
             */

            if (SerialPort.GetPortNames().Length == 0)
                return;
            var rst = new ConcurrentBag<string>();

            var manual = new SerialPortProtocol();

            manual.History.Select(p => $"History: {p}").Subscribe(p => rst.Add(p));
            manual.ConnectionState.Select(p => $"ConnectionState: {p}").Subscribe(p => rst.Add(p));
            manual.Received.Select(p => $"Received: {p.ByteToHex()}").Subscribe(p => rst.Add(p));

            manual.Init(SerialPort.GetPortNames()[0], 9600);

            await manual.OpenAsync();
            await Task.Delay(100);
            await manual.SendAsync(new CommContext("AA00 0a10 afBB", "AA00$2$1BB", 200));
            await Task.Delay(1000);

            if (rst.Count(p => p.StartsWith("Received")) > 0)
            {
                Debugger.Break();
                Assert.True(rst.Count(p => p.Contains("Success")) == 1 && rst.Count(p => p.Contains("Error")) == 0);
            }

            await manual.CloseAsync();
            manual.Dispose();
            rst.Clear();
        }

        private async Task AutoTest()
        {
            /*
             * 需要硬件环境
             * rs232连接2-3, 自发自回
             */

            if (SerialPort.GetPortNames().Length == 0)
                return;
            var rst = new ConcurrentBag<string>();


            var auto = new SerialPortProtocol(1000);

            Observable.Merge
                (
                    auto.History.Select(p => $"History: {p}"),
                    auto.ConnectionState.Select(p => $"ConnectionState: {p}"),
                    auto.Received.Select(p => $"Received: {p.ByteToHex()}")
                )
                .Subscribe(p => { rst.Add(p); });

            auto.Init(SerialPort.GetPortNames()[0], 115200);
            await Task.Delay(100);
            //send when connected
            auto.ConnectionState
                .Where(p => p)
                .Delay(TimeSpan.FromSeconds(0.5))
                .Subscribe(async _ =>
                {
                    await auto.SendAsync(new CommContext("AA00 0888 44BB", "AA00$2$1BB", 200));
                    await auto.SendAsync(new CommContext("CC00 1111 DD", "CC00$2DD", 200));
                });

            await Task.Delay(2000);

            if (rst.Count(p => p.StartsWith("Received")) > 0)
            {
                Debugger.Break();
                Assert.True(rst.Count(p => p.Contains("Success")) == 2 && rst.Count(p => p.Contains("Error")) == 0);
            }

            auto.Dispose();
            rst.Clear();
        }
    }
}