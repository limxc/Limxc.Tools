using Limxc.Tools.DeviceComm.Extensions;
using System;

namespace Limxc.Tools.DeviceComm.Entities
{
    public class CPContext : CPCmd
    {
        /// <summary>
        /// 无返回值
        /// </summary>
        /// <param name="cmdTemplate"></param>
        /// <param name="desc"></param>
        public CPContext(string cmdTemplate, string desc = "") : base(cmdTemplate, "", desc)
        {
            Timeout = 0;
        }

        /// <summary>
        /// 有返回值
        /// </summary>
        /// <param name="cmdTemplate"></param>
        /// <param name="respTemplate"></param>
        /// <param name="timeout"></param>
        /// <param name="desc"></param>
        public CPContext(string cmdTemplate, string respTemplate, int timeout = 1000, string desc = "") : base(cmdTemplate, respTemplate, desc)
        {
            Timeout = timeout;
        }

        /// <summary>
        /// 响应时间(毫秒): 下位机处理并返回结果的时常
        /// 0表明没有返回值
        /// </summary>
        public int Timeout { get; }

        /// <summary>
        /// 解析状态
        /// </summary>
        public CPContextStatus Status { get; internal set; } = CPContextStatus.Waiting;

        public DateTime? SendTime { get; set; }
        public DateTime? ReceivedTime { get; set; }

        public override string ToString()
        {
            return $"Command({Desc}):[{Template.HexStrFormat()}]    |    Status:{Status} Send@{SendTime:hh:mm:ss fff}  Receive@{ReceivedTime:hh:mm:ss fff}    |    {Response?.ToString()}";
        }
    }

    public enum CPContextStatus
    {
        /// <summary>
        /// 等待解析
        /// </summary>
        Waiting = 0,

        /// <summary>
        /// 无需解析
        /// </summary>
        NoNeed = 1,

        /// <summary>
        /// 解析成功
        /// </summary>
        Success = 2,

        /// <summary>
        /// 解析超时(返回值丢失)
        /// </summary>
        Timeout = 3
    }
}