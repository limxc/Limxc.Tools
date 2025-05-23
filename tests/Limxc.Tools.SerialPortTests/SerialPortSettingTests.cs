﻿using FluentAssertions;
using Limxc.Tools.SerialPort;
using Xunit;

namespace Limxc.Tools.SerialPortTests;

public class SerialPortSettingTests
{
    [Fact]
    public void CtorTest()
    {
        new Sps(null, 9600).Check(out _).Should().BeFalse();
        new Sps("test", 9600).Check(out _).Should().BeTrue();
    }

    private class Sps : SerialPortSetting
    {
        public Sps(string portName, int baudRate)
        {
            PortName = portName;
            BaudRate = baudRate;
        }
    }
}