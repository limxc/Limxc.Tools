﻿using System;
using System.IO.Ports;
using System.Text.RegularExpressions;

namespace Limxc.Tools.SerialPort
{
    public class SerialPortSetting
    {
        public SerialPortSetting(string portName, int baudRate)
        {
            if (string.IsNullOrWhiteSpace(portName))
                throw new Exception("串口名未设置");
            PortName = portName.Trim().ToUpper();
            if (!Regex.IsMatch(PortName, @"(?i)^(COM)[1-9][0-9]{0,1}$"))
                throw new Exception($"串口名错误:{PortName}");
            BaudRate = baudRate;
        }

        public string PortName { get; set; }
        public int BaudRate { get; set; }

        public Parity Parity { get; set; } = Parity.None;

        public int DataBits { get; set; } = 8;

        public StopBits StopBits { get; set; } = StopBits.One;

        /// <summary>
        ///     Try Connect Interval ms
        /// </summary>
        public int AutoConnectInterval { get; set; } = 1000;

        /// <summary>
        ///     Delay ms
        /// </summary>
        public int SendDelay { get; set; } = 50;

        /// <summary>
        ///     Is Enabled
        /// </summary>
        public bool Enable { get; set; } = true;
    }
}