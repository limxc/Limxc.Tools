using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using Limxc.Tools.Extensions;

namespace Limxc.Tools.Sockets.Tcp
{
    public class TcpS2CSetting
    {
        public string ServerIpPort { get; set; }
        public string ClientIp { get; set; }

        public bool Enabled { get; set; } = true;

        public bool Check(out string errMsg)
        {
            if (!ServerIpPort.CheckIpPort())
            {
                errMsg = "服务端IP地址端口未设置.";
                return false;
            }

            if (!ClientIp.CheckIp())
            {
                errMsg = "设备端IP地址未设置.";
                return false;
            }

            var existIpPort = NetworkInterface
                .GetAllNetworkInterfaces()
                .Where(p => p.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
                .Select(p =>
                    p.GetIPProperties()
                        .UnicastAddresses.Where(i =>
                            i.Address.AddressFamily == AddressFamily.InterNetwork
                        )
                )
                .SelectMany(p => p.Select(i => i.Address.ToString()))
                .Any(p => ServerIpPort.Contains(p));

            if (!existIpPort)
            {
                errMsg = "系统网卡IP地址未设置.";
                return false;
            }

            errMsg = string.Empty;
            return true;
        }
    }
}