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
            set => _portName = value?.Trim().ToUpper();
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

        public bool Check(out string errMsg)
        {
            if (string.IsNullOrWhiteSpace(PortName))
            {
                errMsg = "串口名未设置";
                return false;
            }

            if (!Regex.IsMatch(PortName, @"(?i)^(COM)[1-9][0-9]{0,1}$"))
            {
                errMsg = $"串口名错误:{PortName}";
                return false;
            }

            if (!AvailableBaudRates.Contains(BaudRate))
            {
                errMsg = $"波特率错误:{BaudRate}";
                return false;
            }

            errMsg = string.Empty;
            return true;
        }
    }
}