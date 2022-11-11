using System;
using System.IO.Ports;
using System.Linq;
using System.Text.RegularExpressions;

namespace Limxc.Tools.SerialPort
{
    public class SerialPortSetting
    {
        private string _portName;

        public string PortName
        {
            get => _portName;
            set => _portName = value.Trim().ToUpper();
        }

        public int BaudRate { get; set; } = 9600;

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

        public virtual int[] AvailableBaudRates { get; } = { 1200, 4800, 9600, 19200, 115200 };

        public void Check()
        {
            if (string.IsNullOrWhiteSpace(PortName))
                throw new Exception("串口名未设置");

            if (!Regex.IsMatch(PortName, @"(?i)^(COM)[1-9][0-9]{0,1}$"))
                throw new Exception($"串口名错误:{PortName}");

            if (!AvailableBaudRates.Contains(BaudRate))
                throw new Exception($"波特率错误:{BaudRate}");
        }
    }
}