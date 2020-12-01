using System;

namespace Limxc.Tools.DeviceComm.Protocol
{
    public class ProcotolConnectionState
    {
        public bool IsConnected { get; set; }

        public string IpPort { get; set; }

        public int Port
        {
            get
            {
                try
                {
                    return Convert.ToInt32(IpPort.Split(':')[1]);
                }
                catch
                {
                    return -1;
                }
            }
        }

        public string Ip
        {
            get
            {
                try
                {
                    return IpPort.Split(':')[0];
                }
                catch
                {
                    return "";
                }
            }
        }
    }
}