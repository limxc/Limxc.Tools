using System;

namespace Limxc.Tools.DeviceComm.Entities
{
    public class CPContext : CPCmd
    {
        public CPContext(string cmdCemplate, string respTemplate, string cmdDesc = "", string respDesc = "") : base(cmdCemplate, respTemplate, cmdDesc, respDesc)
        {
        }

        /// <summary>
        /// 响应时间(毫秒): 下位机处理并返回结果的时常
        /// </summary>
        public int TimeOut { get; set; } = 256;

        public string Id { get; set; }
        public object Data { get; set; }

        public DateTime? SendTime { get; set; }
        public DateTime? ReceivedTime { get; set; }
    }
}