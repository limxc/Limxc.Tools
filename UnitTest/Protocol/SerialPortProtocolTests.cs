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
        public async void SerialPortProtocolTest()
        {
            /*
                串口2-3连接, 自发自回
             */
            var dis = new CompositeDisposable();

            if (SerialPort.GetPortNames().Length == 0)
                return;

            Debug.WriteLine($"****** {nameof(SerialPortProtocol_GS)} Test  ******");
            var sp = new SerialPortProtocol_GS(SerialPort.GetPortNames()[0], 9600);

            var rst = new List<string>();

            await sp.OpenAsync();

            Observable.Merge
                (
                    sp.History.Select(p => $"{DateTime.Now:mm:ss ffff} {p}"),
                    sp.ConnectionState.Select(p => $"{DateTime.Now:mm:ss ffff} 连接状态: {p}"),
                    sp.Received.Select(p => $"{DateTime.Now:mm:ss ffff} receive : {p}")
                )
                .Subscribe(p =>
                {
                    rst.Add(p);
                })
                .DisposeWith(dis);

            await sp.SendAsync(new CPContext("AA00 0a10 afBB", "AA00$2$1BB") { TimeOut = 256 });
            await Task.Delay(1000);

            Assert.True(rst.Count > 0 && rst.Count(p => p.Contains("不匹配")) == 0);

            await sp.CloseAsync();

            dis.Dispose();
            dis = new CompositeDisposable();

            rst.ForEach(p => Debug.WriteLine(p));
            Debugger.Break();
            rst.Clear();

            //-------------
            Debug.WriteLine($"****** {nameof(SerialPortProtocol_SPS)} Test  ******");
            var sps = new SerialPortProtocol_SPS(SerialPort.GetPortNames()[0], 9600);

            await sps.OpenAsync();

            Observable.Merge
                (
                    sps.History.Select(p => $"{DateTime.Now:mm:ss ffff} {p}"),
                    sps.ConnectionState.Select(p => $"{DateTime.Now:mm:ss ffff} 连接状态: {p}"),
                    sps.Received.Select(p => $"{DateTime.Now:mm:ss ffff} receive : {p}")
                )
                .Subscribe(p =>
                {
                    rst.Add(p);
                })
                .DisposeWith(dis);

            await sps.SendAsync(new CPContext("AA00 0a10 afBB", "AA00$2$1BB") { TimeOut = 256 });
            await Task.Delay(1000);

            Assert.True(rst.Count > 0 && rst.Count(p => p.Contains("不匹配")) == 0);

            await sps.CloseAsync();
            dis.Dispose();

            rst.ForEach(p => Debug.WriteLine(p));
            Debugger.Break();
            rst.Clear();

            sp.Dispose();
            sps.Dispose();
        }
    }
}