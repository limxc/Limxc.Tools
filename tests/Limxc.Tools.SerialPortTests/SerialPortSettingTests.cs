using System;
using Limxc.Tools.SerialPort;
using Xunit;

namespace Limxc.Tools.SerialPortTests;

public class SerialPortSettingTests
{
    [Fact]
    public void CtorTest()
    {
        Assert.Throws<Exception>(() => new Sps(null, 9600));
        Assert.Throws<Exception>(() => new Sps("test", 9600));
    }

    private class Sps : SerialPortSetting
    {
        public Sps(string portName, int baudRate) : base(portName,
            baudRate)
        {
        }
    }
}