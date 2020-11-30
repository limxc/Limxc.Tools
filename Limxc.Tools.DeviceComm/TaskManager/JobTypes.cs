namespace Limxc.Tools.DeviceComm.TaskManager
{
    /// <summary>
    /// 任务状态
    /// </summary>
    public enum JobState
    {
        /// <summary>
        /// 未初始化的默认状态
        /// </summary>
        UnSet = 0,

        //就绪 = 1,
        //执行中 = 2,
        //暂停 = 3,
        完成 = 4,

        失败 = 5
    }

    /// <summary>
    /// 任务调度执行状态
    /// </summary>
    public enum JobSchedulerState
    {
        就绪 = 0,
        执行中 = 1,
        停止中 = 2
    }

    /// <summary>
    /// 任务调度事件类型
    /// </summary>
    public enum JobSchedulerEventType
    {
        指令事件 = 0,
        调度器状态 = 1,
        任务列表改变 = 2
    }
}