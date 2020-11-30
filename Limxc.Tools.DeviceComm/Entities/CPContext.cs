using System;

namespace Limxc.Tools.DeviceComm.Entities
{
    public class CPContext : CPCmd
    {
        /// <summary>
        /// 初始化指令模板,占位符 $n n=1-9位 length=n*2
        /// </summary>
        /// <param name="command"></param>
        /// <param name="respTemplate"></param>
        /// <param name="timeout">读取延时(毫秒):读取完整数据所需时间,每条指令不同</param>
        /// <param name="cmdDesc"></param>
        /// <param name="respDesc"></param>
        public CPContext

            (string command, string respTemplate, int timeout, string cmdDesc = "", string respDesc = "") : base(command, respTemplate, cmdDesc, respDesc)
        {
            TimeOut = timeout;
            Id = Guid.NewGuid().ToString("N");
        }

        /// <summary>
        /// 初始化指令模板,占位符 $n n=1-9位 length=n*2
        /// 无返回值
        /// </summary>
        /// <param name="command"></param>
        /// <param name="cmdDesc"></param>
        public CPContext(string command, string cmdDesc = "") : this(command, "", 0, cmdDesc, "")
        {
            Id = Guid.NewGuid().ToString("N");
        }

        /// <summary>
        /// 读取超时(毫秒):读取完整数据所需时间,每条指令不同
        /// </summary>
        public int TimeOut { get; }

        public string Id { get; set; }

        public DateTime? SendTime { get; set; }
        public DateTime? ReceivedTime { get; set; }
    }
}