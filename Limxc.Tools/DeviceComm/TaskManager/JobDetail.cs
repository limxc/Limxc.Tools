
using Limxc.Tools.DeviceComm.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Limxc.Tools.DeviceComm.TaskManager
{
    public class JobDetail
    {
        #region 初始化

        public Guid Id { get; set; }

        public JobDetail()
        {
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="command"></param>
        /// <param name="execInterval">执行间隔</param>
        /// <param name="isCrucialPoint">是否为关键点,中断后续操作</param>
        /// <param name="repeatExecCount">重复执行次数,将自动加入待执行队列.默认0,只执行一次,不重复.</param> <
        public JobDetail(CPCmd command, bool isCrucialPoint = false, int repeatExecCount = 0, int execInterval = 0)
        {
            Id = Guid.NewGuid();
            IsCrucialPoint = isCrucialPoint;
            Command = command;
            RepeatExecCount = repeatExecCount;
            ExecInterval = execInterval;
        }

        /// <summary>
        /// 指令
        /// </summary>
        public CPCmd Command { get; private set; }

        /// <summary>
        /// 命令执行处理器
        /// </summary>
        public Func<CPCmd, string> ExecHandler { get; private set; }

        /// <summary>
        /// 命令执行结果处理器
        /// </summary>
        public Func<CommResp, JobState> CallbackHandler { get; private set; }

        /// <summary>
        /// 任务执行事件列表
        /// </summary>
        public List<JobEvent> Events { get; private set; } = new List<JobEvent>();

        public int ExecInterval { get; private set; }

        #endregion 初始化

        #region 业务属性

        /// <summary>
        /// 重复执行次数.默认0,只执行一次,不重复.
        /// </summary>
        public long RepeatExecCount { get; set; }

        /// <summary>
        /// 是否为关键点,中断后续操作
        /// </summary>
        public bool IsCrucialPoint { get; private set; } = false;

        /// <summary>
        /// 致命失败,中断后续操作
        /// </summary>
        public bool CanContinue => !(!CanExec && IsCrucialPoint && State != JobState.完成);

        /// <summary>
        /// 最终执行状态
        /// </summary>
        public JobState State
        {
            get
            {
                if (Events.Count <= 0)
                    return default;

                var rst = Events.First(p => p.ExecTime == Events.Max(c => c.ExecTime)).State;
                return rst;
            }
        }

        /// <summary>
        /// 最后执行时间
        /// </summary>
        public DateTime? LatestExecDateTime
        {
            get
            {
                if (Events.Count <= 0)
                    return null;

                var rst = Events.First(p => p.ExecTime == Events.Max(c => c.ExecTime)).ExecTime;
                return rst;
            }
        }

        /// <summary>
        /// 执行次数
        /// </summary>
        public int ExecedCount => Events.Count;

        /// <summary>
        /// 是否可执行
        /// </summary>
        public bool CanExec
        {
            get
            {
                return ExecHandler != null && Command.RetryCount > ExecedCount-1 && State != JobState.完成;
            }
        }

        #endregion 业务属性

        #region 业务方法

        /// <summary>
        /// 重置为原始状态,但RepeatExecCount-1
        /// </summary>
        /// <returns>
        /// </returns>
        public void Reset()
        {
            if (RepeatExecCount > 0)
                RepeatExecCount--;
            Events.Clear();
        }

        #endregion 业务方法

        #region 处理程序

        /// <summary>
        /// 发送指令并返回结果
        /// </summary>
        /// <param name="handler">
        /// 返回通信结果
        /// </param>
        /// <param name="overWrite">
        /// 是否覆盖
        /// </param>
        /// <returns>
        /// </returns>
        public JobDetail UseExecHandler(Func<CPCmd, string> handler, bool overWrite = false)
        {
            if (overWrite || ExecHandler == null)
                ExecHandler = handler;
            return this;
        }

        /// <summary> 对原始结果处理并返回执行状态 </summary> <param name="handler">处理原始通信结果<see
        /// cref="CPResp.Value">,格式化</param> <param name="overWrite">是否覆盖</param>
        /// <returns></returns>
        public JobDetail UseCallback(Func<CommResp, JobState> handler, bool overWrite = false)
        {
            if (overWrite || CallbackHandler == null)
                CallbackHandler = handler;
            return this;
        }

        #endregion 处理程序

        public override string ToString()
        {
            return $"JobDetail:[ {Id} 已执行:{ExecedCount} ]"
                + Environment.NewLine
                + Command.ToString();
        }
    }
}