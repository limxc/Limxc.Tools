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
        /// <param name="timeout">响应时间(毫秒): 设备数据处理及返回总耗时, 建议大于128ms</param>
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
        /// 接收延时(毫秒): 下位机处理并返回结果的时常
        /// </summary>
        public int TimeOut { get; }

        public string Id { get; set; }

        public DateTime? SendTime { get; set; }
        public DateTime? ReceivedTime { get; set; }
    }
}