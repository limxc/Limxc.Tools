using System;
using FluentAssertions;
using Limxc.Tools.SerialPort;
using Xunit;

namespace Limxc.Tools.SerialPortTests;

public class SerialPortSettingBaseTests
{
    [Fact]
    public void CtorTest()
    {
        Assert.Throws<Exception>(() => new Sps(null, 9600));
        Assert.Throws<Exception>(() => new Sps("test", 9600));
    }

    [Fact]
    public void EqualityTest()
    {
        var s1 = new Sps("Com1", 9600);
        var s2 = new Sps("COM1 ", 9600, 1500, 150);
        var s3 = new Sps(" COM1", 4800);
        var s4 = new Sps(" Com1 ", 4800);
        (s1 == s2).Should().BeTrue();
        (s1 == s3).Should().BeFalse();
        (s1 == s4).Should().BeFalse();
    }

    private class Sps : SerialPortSettingBase
    {
        public Sps(string portName, int baudRate, int autoConnectInterval = 1000, int sendDelay = 50) : base(portName,
            baudRate, autoConnectInterval, sendDelay)
        {
        }
    }
}