using Limxc.Tools.DeviceComm.Entities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Limxc.Tools.DeviceComm.Protocol.Tests
{
    public class SerialPortProtocolTests
    {
        [Fact()]
        public async Task SerialPortProtocolTest()
        {
            /*
             * 需要硬件环境
             * rs232连接2-3, 自发自回
             */
            var dis = new CompositeDisposable();

            if (SerialPort.GetPortNames().Length == 0)
                return;

            Debug.WriteLine($"****** {nameof(SerialPortProtocol_GS)} Test  ******");
            IProtocol sp = new SerialPortProtocol_GS(SerialPort.GetPortNames()[0], 9600);

            var rst = new List<string>();

            var opened = await sp.OpenAsync();

            if (!opened && !Debugger.IsAttached)
                return;

            Observable.Merge
                (
                    sp.History.Select(p => $"{DateTime.Now:mm:ss fff} {p}"),
                    sp.ConnectionState.Select(p => $"{DateTime.Now:mm:ss fff} 连接状态: {p}"),
                    sp.Received.Select(p => $"{DateTime.Now:mm:ss fff} receive : {p}")
                )
                .Subscribe(p =>
                {
                    rst.Add(p);
                })
                .DisposeWith(dis);

            await sp.SendAsync(new CPContext("AA00 0a10 afBB", "AA00$2$1BB", 256));
            await Task.Delay(1000);
            await sp.CloseAsync();

            Assert.True(rst.Count == 5 && rst.Count(p => p.Contains("Error")) == 0);

            dis.Dispose();
            dis = new CompositeDisposable();

            rst.ForEach(p => Debug.WriteLine(p));
            Debugger.Break();
            rst.Clear();

            //-------------
            Debug.WriteLine($"****** {nameof(SerialPortProtocol_SPS)} Test  ******");
            IProtocol sps = new SerialPortProtocol_SPS(SerialPort.GetPortNames()[0], 9600);

            await sps.OpenAsync();

            Observable.Merge
                (
                    sps.History.Select(p => $"{DateTime.Now:mm:ss fff} {p}"),
                    sps.ConnectionState.Select(p => $"{DateTime.Now:mm:ss fff} 连接状态: {p}"),
                    sps.Received.Select(p => $"{DateTime.Now:mm:ss fff} receive : {p}")
                )
                .Subscribe(p =>
                {
                    rst.Add(p);
                })
                .DisposeWith(dis);

            await sps.SendAsync(new CPContext("AA00 0a10 afBB", "AA00$2$1BB", 256));
            await Task.Delay(1000);
            await sps.CloseAsync();

            Assert.True(rst.Count == 5 && rst.Count(p => p.Contains("Error")) == 0);

            dis.Dispose();

            rst.ForEach(p => Debug.WriteLine(p));
            Debugger.Break();
            rst.Clear();

            sp.CleanUp();
            sps.CleanUp();
        }
    }
}