using System;

namespace Limxc.Tools.DeviceComm.TaskManager
{
    public class JobEvent
    {
        public Guid Id { get; private set; } = Guid.NewGuid();
        public Guid JobDetailId { get; private set; }

        public JobEvent()
        {
        }

        public JobEvent(Guid jobDetailId, string result, JobState state, DateTime execTime)
        {
            JobDetailId = jobDetailId;
            Result = result;
            State = state;
            ExecTime = execTime;
        }

        /// <summary>
        /// 任务执行数据
        /// </summary>
        public string Result { get; private set; }

        /// <summary>
        /// 任务执行状态
        /// </summary>
        public JobState State { get; private set; }

        /// <summary>
        /// 执行时间
        /// </summary>
        public DateTime ExecTime { get; private set; }

        public override string ToString()
        {
            return $"JobEvent:[ {State.ToString()} @ {ExecTime} - {Result} ]";
        }
    }
}