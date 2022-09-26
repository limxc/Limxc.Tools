using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text.RegularExpressions;

namespace Limxc.Tools.SerialPort
{
    public abstract class SerialPortSettingBase
    {
        protected SerialPortSettingBase(string portName, int baudRate, int autoConnectInterval = 1000,
            int sendDelay = 50)
        {
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
        ///     interval ms
        /// </summary>
        public int AutoConnectInterval { get; set; }

        /// <summary>
        ///     delay ms
        /// </summary>
        public int SendDelay { get; set; }

        #region Equality

        protected static bool EqualOperator(SerialPortSettingBase left, SerialPortSettingBase right)
        {
            if (left is null ^ right is null) return false;

            return left?.Equals(right) != false;
        }

        protected static bool NotEqualOperator(SerialPortSettingBase left, SerialPortSettingBase right)
        {
            return !EqualOperator(left, right);
        }


        private IEnumerable<object> GetEqualityComponents()
        {
            yield return PortName;
            yield return BaudRate;
            yield return Parity;
            yield return DataBits;
            yield return StopBits;

            yield return AutoConnectInterval;
            yield return SendDelay;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != GetType()) return false;

            var other = (SerialPortSettingBase)obj;
            return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
        }

        public static bool operator ==(SerialPortSettingBase one, SerialPortSettingBase two)
        {
            return Equals(one, two);
        }

        public static bool operator !=(SerialPortSettingBase one, SerialPortSettingBase two)
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