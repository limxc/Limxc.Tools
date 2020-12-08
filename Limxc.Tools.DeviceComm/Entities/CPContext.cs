using Limxc.Tools.DeviceComm.Extensions;
using System;

namespace Limxc.Tools.DeviceComm.Entities
{
    public class CPContext : CPCmd
    {
        public CPContext(string cmdCemplate, string respTemplate, string cmdDesc = "", string respDesc = "") : base(cmdCemplate, respTemplate, cmdDesc, respDesc)
        {
        }

        /// <summary>
        /// 解析状态
        /// </summary>
        public CPContextStatus Status { get; set; } = CPContextStatus.Waiting;

        /// <summary>
        /// 响应时间(毫秒): 下位机处理并返回结果的时常
        /// </summary>
        public int Timeout { get; set; } = 1000;

        public string Id { get; set; }

        /// <summary>
        /// 重试次数( +1 = 运行次数 )
        /// </summary>
        public int RetryTimes { get; set; } = 0;

        public object Data { get; set; }

        public DateTime? SendTime { get; set; }
        public DateTime? ReceivedTime { get; set; }

        public override string ToString()
        {
            return $"Command({Desc}):[{Template.HexStrFormat()}]    |    {Status}(Send@{SendTime:hh:mm:ss fff}  Receive@{ReceivedTime:hh:mm:ss fff})    |    {Response?.ToString()}";
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