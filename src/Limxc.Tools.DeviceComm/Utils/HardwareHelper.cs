using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Management;

namespace Limxc.Tools.DeviceComm.Utils
{
    /// <summary>
    /// 硬件信息助手类
    /// </summary>
    public static class HardwareHelper
    {
        public static string[] Hardwares => GetHardWare(HardwareEnum.Win32_PnPEntity);

        public static string[] SerialPorts => GetHardWare(HardwareEnum.Win32_SerialPort);

        /// <summary>
        /// 获取匹配设备
        /// </summary>
        /// <param name="deviceName"></param>
        /// <returns></returns>
        public static string GetPortNameBy(string deviceName)
        {
            var comStr = SerialPorts.FirstOrDefault(p => p.Contains(deviceName));

            if (!string.IsNullOrWhiteSpace(comStr))
            {
                string[] sArray = comStr.Split(new char[2] { '(', ')' });
                if (sArray.Length >= 2)
                {
                    comStr = sArray[1];
                    return SerialPort.GetPortNames().FirstOrDefault(p => p.Contains(comStr));
                }
            }
            //未找到匹配设备
            return null;
        }

        /// <summary>
        /// WMI取硬件信息
        /// </summary>
        /// <param name="hardType">硬件类型</param>
        /// <param name="propKey">筛选范围,默认为属性名</param>
        /// <param name="propValue">筛选条件,默认为COM</param>
        /// <returns></returns>
        public static string[] GetHardWare(HardwareEnum hardType, string propKey = "Name", string propValue = "COM")
        {
            List<string> strs = new List<string>();
            try
            {
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("select * from " + hardType))
                {
                    foreach (var hardInfo in searcher.Get())
                    {
                        if (hardInfo.Properties[propKey].Value != null)
                        {
                            if (hardInfo.Properties[propKey].Value.ToString().Contains(propValue))
                            {
                                strs.Add(hardInfo.Properties[propKey].Value.ToString());
                            }
                        }
                    }
                }
                return strs.ToArray();
            }
            catch
            {
                return null;
            }
        }
    }

    /// <summary>
    /// 硬件枚举
    /// </summary>
    public enum HardwareEnum
    {
        // 硬件
        Win32_Processor, // CPU 处理器

        Win32_PhysicalMemory, // 物理内存条
        Win32_Keyboard, // 键盘
        Win32_PointingDevice, // 点输入设备，包括鼠标。
        Win32_FloppyDrive, // 软盘驱动器
        Win32_DiskDrive, // 硬盘驱动器
        Win32_CDROMDrive, // 光盘驱动器
        Win32_BaseBoard, // 主板
        Win32_BIOS, // BIOS 芯片
        Win32_ParallelPort, // 并口
        Win32_SerialPort, // 串口
        Win32_SerialPortConfiguration, // 串口配置
        Win32_SoundDevice, // 多媒体设置，一般指声卡。
        Win32_SystemSlot, // 主板插槽 (ISA & PCI & AGP)
        Win32_USBController, // USB 控制器
        Win32_NetworkAdapter, // 网络适配器
        Win32_NetworkAdapterConfiguration, // 网络适配器设置
        Win32_Printer, // 打印机
        Win32_PrinterConfiguration, // 打印机设置
        Win32_PrintJob, // 打印机任务
        Win32_TCPIPPrinterPort, // 打印机端口
        Win32_POTSModem, // MODEM
        Win32_POTSModemToSerialPort, // MODEM 端口
        Win32_DesktopMonitor, // 显示器
        Win32_DisplayConfiguration, // 显卡
        Win32_DisplayControllerConfiguration, // 显卡设置
        Win32_VideoController, // 显卡细节。
        Win32_VideoSettings, // 显卡支持的显示模式。

        // 操作系统
        Win32_TimeZone, // 时区

        Win32_SystemDriver, // 驱动程序
        Win32_DiskPartition, // 磁盘分区
        Win32_LogicalDisk, // 逻辑磁盘
        Win32_LogicalDiskToPartition, // 逻辑磁盘所在分区及始末位置。
        Win32_LogicalMemoryConfiguration, // 逻辑内存配置
        Win32_PageFile, // 系统页文件信息
        Win32_PageFileSetting, // 页文件设置
        Win32_BootConfiguration, // 系统启动配置
        Win32_ComputerSystem, // 计算机信息简要
        Win32_OperatingSystem, // 操作系统信息
        Win32_StartupCommand, // 系统自动启动程序
        Win32_Service, // 系统安装的服务
        Win32_Group, // 系统管理组
        Win32_GroupUser, // 系统组帐号
        Win32_UserAccount, // 用户帐号
        Win32_Process, // 系统进程
        Win32_Thread, // 系统线程
        Win32_Share, // 共享
        Win32_NetworkClient, // 已安装的网络客户端
        Win32_NetworkProtocol, // 已安装的网络协议
        Win32_PnPEntity,//all device
    }

    /// <summary>
    /// 串口波特率列表。
    /// 75,110,150,300,600,1200,2400,4800,9600,14400,19200,28800,38400,56000,57600,
    /// 115200,128000,230400,256000
    /// </summary>
    public enum BaudRateEnum : int
    {
        BaudRate_75 = 75,
        BaudRate_110 = 110,
        BaudRate_150 = 150,
        BaudRate_300 = 300,
        BaudRate_600 = 600,
        BaudRate_1200 = 1200,
        BaudRate_2400 = 2400,
        BaudRate_4800 = 4800,
        BaudRate_9600 = 9600,
        BaudRate_14400 = 14400,
        BaudRate_19200 = 19200,
        BaudRate_28800 = 28800,
        BaudRate_38400 = 38400,
        BaudRate_56000 = 56000,
        BaudRate_57600 = 57600,
        BaudRate_115200 = 115200,
        BaudRate_128000 = 128000,
        BaudRate_230400 = 230400,
        BaudRate_256000 = 256000
    }

    /// <summary>
    /// 串口数据位列表（5,6,7,8）
    /// </summary>
    public enum DataBitsEnum : int
    {
        FiveBits = 5,
        SixBits = 6,
        SeventBits = 7,
        EightBits = 8
    }
}