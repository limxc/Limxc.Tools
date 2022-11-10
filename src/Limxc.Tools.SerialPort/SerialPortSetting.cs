using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text.RegularExpressions;

namespace Limxc.Tools.SerialPort
{
    public class SerialPortSetting
    {
        public SerialPortSetting()
        {
        }

        public SerialPortSetting(string portName, int baudRate, int autoConnectInterval = 1000,
            int sendDelay = 50)
        {
            if (string.IsNullOrWhiteSpace(portName))
                throw new Exception("串口名未设置");
            PortName = portName.Trim().ToUpper();
            if (!Regex.IsMatch(PortName, @"(?i)^(COM)[1-9][0-9]{0,1}$"))
                throw new Exception($"串口名错误:{PortName}");
            BaudRate = baudRate;
            AutoConnectInterval = autoConnectInterval;
            SendDelay = sendDelay;
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

        #region Equality

        private IEnumerable<object> GetEqualityComponents()
        {
            yield return PortName;
            yield return BaudRate;
            yield return Parity;
            yield return DataBits;
            yield return StopBits;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != GetType()) return false;

            var other = (SerialPortSetting)obj;
            return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
        }

        public static bool operator ==(SerialPortSetting one, SerialPortSetting two)
        {
            return Equals(one, two);
        }

        public static bool operator !=(SerialPortSetting one, SerialPortSetting two)
        {
            return !Equals(one, two);
        }

        public override int GetHashCode()
        {
            return GetEqualityComponents()
                .Select(x => x != null ? x.GetHashCode() : 0)
                .Aggregate((x, y) => x ^ y);
        }

        #endregion
    }
}