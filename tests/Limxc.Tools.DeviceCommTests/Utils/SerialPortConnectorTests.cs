using System.IO.Ports;
using System.Linq;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using FluentAssertions;
using Limxc.Tools.DeviceComm.Utils;
using Limxc.Tools.Extensions.Communication;
using Xunit;

namespace Limxc.Tools.DeviceCommTests.Utils
{
    [Collection("SerialPort")]
    public class SerialPortConnectorTests
    {
        [Fact]
        public async Task SendAsyncTest()
        {
            /*
             * 需要硬件环境
             * rs232连接2-3, 自发自回
             */

            if (SerialPort.GetPortNames().Length == 0)
                return;

            var portName = SerialPort.GetPortNames().First();
            var baudRate = 9600;

            var disposables = new CompositeDisposable();

            var sp = new SerialPortConnector();
            sp.IsConnected.Should().BeFalse();
            sp.Init(portName, baudRate);

            await Task.Delay(1000);

            sp.IsConnected.Should().BeTrue();
            await sp.SendAsync("CC");

            var rst1 = await sp.SendAsync("DD00DD", 1000);
            rst1.ByteToHex().Should().Be("DD00DD");

            var rst2 = await sp.SendAsync("AA010203BB", 1000, "AA$1$2BB");
            rst2.Should().Be("AA010203BB");

            var rst3 = await sp.SendAsync("AA 11 22 FF", 1000, "AA1122FF");
            rst3.Should().Be("AA1122FF");
            
            var t1 = sp.SendAsync("00AA01", 3000, "AA$1$2BB");
            await Task.Delay(10);
            var t2 = sp.SendAsync("0203BB0000");
            await Task.WhenAll(t1, t2);
            (await t1).Should().Be("AA010203BB");

            disposables.Dispose();
            sp.Dispose();
        }

        [Fact]
        public async Task ConnectionEnabledTest()
        {
            if (SerialPort.GetPortNames().Length == 0)
                return;

            var portName = SerialPort.GetPortNames().First();
            var baudRate = 9600;

            var disposables = new CompositeDisposable();

            var sp = new SerialPortConnector();
            sp.IsConnected.Should().BeFalse();

            sp.ConnectionEnabled = false;
            sp.Init(portName, baudRate);
            await Task.Delay(1000);
            sp.IsConnected.Should().BeFalse();

            sp.ConnectionEnabled = true;
            await Task.Delay(1000);
            sp.IsConnected.Should().BeTrue();

            sp.ConnectionEnabled = false;
            await Task.Delay(1000);
            sp.IsConnected.Should().BeFalse();

            disposables.Dispose();
            sp.Dispose();
        }
    }
}