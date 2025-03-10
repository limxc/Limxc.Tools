using System.Linq;

namespace Limxc.Tools.SerialPort
{
    public class SerialPortSetting
    {
        public virtual string PortName { get; set; }

        public virtual int BaudRate { get; set; } = 9600;

        /// <summary>
        ///     Default: 4096
        ///     ReadBufferSize >= DataPerSecond/100 (min:4096)
        ///     1M/s => 1024*1024/100 ≈ 10240
        ///     <see cref="SerialPortService.Start" />
        /// </summary>
        public virtual int ReadBufferSize => 4096;

        /// <summary>
        ///     Try To Connect Interval ms
        /// </summary>
        public virtual int AutoConnectInterval { get; set; } = 1000;

        public virtual bool AutoConnectEnabled { get; set; } = true;

        /// <summary>
        ///     Delay ms
        /// </summary>
        public virtual int SendDelay { get; set; } = 50;

        public virtual bool Enabled { get; set; } = true;

        public virtual int[] AvailableBaudRates { get; } = { 1200, 4800, 9600, 19200, 115200 };

        public bool Check(out string errMsg)
        {
            if (string.IsNullOrWhiteSpace(PortName))
            {
                errMsg = "串口名未设置";
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