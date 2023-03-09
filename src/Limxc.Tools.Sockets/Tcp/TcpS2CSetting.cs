using System.Linq;
using System.Net;
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

            var existIpPort = Dns.GetHostEntry(Dns.GetHostName()).AddressList
                .Where(p => p.AddressFamily.ToString() == "InterNetwork")
                .Any(p => ServerIpPort.Contains(p.ToString()));

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