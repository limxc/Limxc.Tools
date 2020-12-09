using Limxc.Tools.DeviceComm.Extensions;

namespace Limxc.Tools.DeviceComm.Entities
{
    public class CPTaskContext : CPContext
    {
        /// <summary>
        /// 无返回值
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cmdTemplate"></param>
        /// <param name="remainTimes">可执行次数=重试次数+1</param>
        /// <param name="desc"></param>
        public CPTaskContext(string id, string cmdTemplate, int remainTimes = 1, string desc = "") : base(cmdTemplate, desc)
        {
            Id = id;
            RemainTimes = remainTimes;
        }

        /// <summary>
        /// 有返回值
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cmdTemplate"></param>
        /// <param name="respTemplate"></param>
        /// <param name="timeout">响应时间</param>
        /// <param name="remainTimes">可执行次数=重试次数+1</param>
        /// <param name="desc"></param>
        public CPTaskContext(string id, string cmdTemplate, string respTemplate, int timeout = 1000, int remainTimes = 1, string desc = "") : base(cmdTemplate, respTemplate, timeout, desc)
        {
            Id = id;
            RemainTimes = remainTimes;
        }

        /// <summary>
        /// 唯一标识
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// 执行次数
        /// </summary>
        public int RemainTimes { get; internal set; }

        public override string ToString()
        {
            return $"Command({Desc}):[{Template.HexStrFormat()}]    |    Id:{Id} RemainTimes:{RemainTimes} Status:{Status} (Send@{SendTime:hh:mm:ss fff}  Receive@{ReceivedTime:hh:mm:ss fff})    |    {Response?.ToString()}";
        }
    }
}