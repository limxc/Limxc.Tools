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
            IProtocol sp = new SerialPortProtocol_GS();

            var rst = new List<string>();

            await sp.Connect(SerialPort.GetPortNames()[0], 9600);

            Observable.Merge
                (
                    sp.History.Select(p => $"{DateTime.Now:mm:ss ffff} {p}"), 
                    sp.IsConnected.Select(p => $"{DateTime.Now:mm:ss ffff} 连接状态: {p}"), 
                    sp.Received.Select(p=>$"{DateTime.Now:mm:ss ffff} receive : {p}")
                )
                .Subscribe(p =>
                {
                    rst.Add(p);
                })
                .DisposeWith(dis);

            await sp.Send(new CPContext("AA00 0a10 afBB", "AA00$2$1BB", 256));
            await Task.Delay(1000);

            Assert.True(rst.Count > 0 && rst.Count(p => p.Contains("数据格式错误")) == 0);

            rst.ForEach(p => Debug.WriteLine(p));
            rst.Clear();
            await sp.Disconnect();

            //-------------
            Debug.WriteLine($"****** {nameof(SerialPortProtocol_SPS)} Test  ******");
            IProtocol sps = new SerialPortProtocol_SPS();
             
            await sps.Connect(SerialPort.GetPortNames()[0], 9600);

            Observable.Merge
                (
                    sps.History.Select(p => $"{DateTime.Now:mm:ss ffff} {p}"),
                    sps.IsConnected.Select(p => $"{DateTime.Now:mm:ss ffff} 连接状态: {p}"),
                    sps.Received.Select(p => $"{DateTime.Now:mm:ss ffff} receive : {p}")
                )
                .Subscribe(p =>
                {
                    rst.Add(p);
                })
                .DisposeWith(dis);

            await sps.Send(new CPContext("AA00 0a10 afBB", "AA00$2$1BB", 256));
            await Task.Delay(1000);

            Assert.True(rst.Count > 0 && rst.Count(p => p.Contains("数据格式错误")) == 0);

            rst.ForEach(p => Debug.WriteLine(p));
            rst.Clear();
            await sps.Disconnect();
             
            dis.Dispose();
            sp.Dispose();
            sps.Dispose();

            Debugger.Break();
        }
    }
}