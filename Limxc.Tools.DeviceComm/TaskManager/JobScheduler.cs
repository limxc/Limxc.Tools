using Limxc.Tools.Extensions;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Limxc.Tools.DeviceComm.TaskManager
{
    /// <summary>
    /// 任务调度器
    /// </summary>
    public class JobScheduler
    {
        #region 初始化

        #region 线程锁 & 单例

        private static readonly Lazy<JobScheduler> lazy =
            new Lazy<JobScheduler>(() => new JobScheduler());

        private JobScheduler()
        {
        }

        public static JobScheduler Instance => lazy.Value;

        private static readonly object locker = new object();

        #endregion 线程锁 & 单例

        /// <summary>
        /// 任务队列
        /// </summary>
        public ConcurrentQueue<JobDetail> Jobs { get; private set; } = new ConcurrentQueue<JobDetail>();

        /// <summary>
        /// 完成列表
        /// </summary>
        public ConcurrentQueue<JobDetail> FinishedJobs { get; private set; } = new ConcurrentQueue<JobDetail>();

        #region 事件

        public delegate void JobFinishedDelegate(JobDetail finishedJob);

        public event JobFinishedDelegate OnJobFinished;

        public delegate void ExecStateChangedDelegate(JobSchedulerState state);

        public event ExecStateChangedDelegate OnExecStateChanged;

        public delegate void LoggerDelegate(string msg, JobSchedulerEventType eventType);

        public event LoggerDelegate OnLog;

        #endregion 事件

        private JobSchedulerState tState;

        /// <summary>
        /// 状态锁
        /// </summary>
        private JobSchedulerState TState
        {
            get => tState;
            set
            {
                OnLog?.Invoke($"----- 调度器状态改变: From[{tState.ToString()}] -> To[{value.ToString()}] -----", JobSchedulerEventType.调度器状态);
                tState = value;
                OnExecStateChanged?.Invoke(value);
            }
        }

        #endregion 初始化

        #region 业务属性

        /// <summary>
        /// 最大数据存储量
        /// </summary>
        public const int MaxStorageCount = 1000;

        /// <summary>
        /// 任务统计信息
        /// </summary>
        public (long Undo, int Done) Statics
        {
            get
            {
                var jobCount = Jobs.Sum(p => p.RepeatExecCount + 1);
                var finishedCount = FinishedJobs.Count();
                return (jobCount, finishedCount);
            }
        }

        /// <summary>
        /// 所有任务是否已完成
        /// </summary>
        public bool IsFinished => Jobs.Count == 0;

        #endregion 业务属性

        #region 业务方法

        /// <summary>
        /// 核心方法
        /// </summary>
        /// <returns>
        /// </returns>
        private Task Exec()
        {
            JobDetail jd;
            //移除过量数据
            void RemoveExcessFinishedJobs()
            {
                JobDetail tmp;
                if (FinishedJobs.Count > MaxStorageCount)
                {
                    FinishedJobs.TryDequeue(out tmp);
                    tmp = null;
                }
            }

            //业务处理及失败重试
            void ExecJob()
            {
                if (!Jobs.TryPeek(out jd))
                    return;

                while (TState != JobSchedulerState.停止中 && jd.CanExec)
                {
                    var state = JobState.完成;
                    var value = "";
                    try
                    {
                        if (jd.ExecInterval > 0)
                            Thread.Sleep(jd.ExecInterval);

                        value = jd.ExecHandler.Invoke(jd.Command);
                        jd.Command.Response.Value = value;

                        OnLog($"发送指令:[{jd.Command.ToCommand()}]  响应值:[{value}]", JobSchedulerEventType.指令事件);

                        if (jd.CallbackHandler != null)
                            state = jd.CallbackHandler.Invoke(jd.Command.Response);
                    }
                    catch (Exception e)
                    {
                        value = $"Error: {e.Message}";

                        OnLog(value, JobSchedulerEventType.指令事件);

                        state = JobState.失败;
                    }
                    finally
                    {
                        jd.Events.Add(new JobEvent(jd.Id, value, state, DateTime.Now));
                    }
                }
            }

            return Task.Run(() =>
                 {
                     while (TState != JobSchedulerState.停止中 && Jobs.Count > 0)
                     {
                         ExecJob();

                         RemoveExcessFinishedJobs();

                         #region 重复执行及关键点失败退出

                         /*
                          * 1.有重复次数
                          * 2.关键点,失败不许继续
                          * 优先级1>2
                          */
                         if (jd.RepeatExecCount > 0)
                         {
                             //入队改为ExecJob中暂存的深复制
                             FinishedJobs.Enqueue(jd.DeepCopy());
                             //原任务重试次数-1
                             jd.RepeatExecCount--;

                             //记录日志
                             OnLog($"执行完毕: @ {jd.LatestExecDateTime} {jd.Command.Response.Value}", JobSchedulerEventType.指令事件);
                             OnLog($"待执行指令:[{Statics.Undo}] 已完成:[{Statics.Done}]{Environment.NewLine}", JobSchedulerEventType.指令事件);

                             OnJobFinished?.Invoke(jd);

                             //状态重置
                             jd.Events.Clear();

                             //忽略关键点
                             continue;
                         }
                         else
                         {
                             //普通出入队
                             FinishedJobs.Enqueue(jd);
                             Jobs.TryDequeue(out jd);

                             //记录日志
                             OnLog($"执行完毕: @ {jd.LatestExecDateTime} {jd.Command.Response.Value}", JobSchedulerEventType.指令事件);
                             OnLog($"待执行指令:[{Statics.Undo}] 已完成:[{Statics.Done}]{Environment.NewLine}", JobSchedulerEventType.指令事件);

                             OnJobFinished?.Invoke(jd);
                         }

                         //关键点失败,跳过后续操作
                         if (!jd.CanContinue)
                             break;

                         #endregion 重复执行及关键点失败退出
                     }
                     lock (locker)
                     {
                         TState = JobSchedulerState.就绪;
                     }
                 });
        }

        /// <summary>
        /// 启动任务队列
        /// </summary>
        /// <returns>
        /// </returns>
        public async Task Start()
        {
            if (TState != JobSchedulerState.就绪)
                return;

            lock (locker)
            {
                TState = JobSchedulerState.执行中;
            }
            await Exec();
        }

        /// <summary>
        /// 停止任务队列
        /// </summary>
        /// <returns>
        /// </returns>
        public async Task Stop()
        {
            if (TState != JobSchedulerState.执行中)
                return;
            lock (locker)
            {
                TState = JobSchedulerState.停止中;
            }
            await Task.Run(async () =>
            {
                while (TState != JobSchedulerState.就绪)
                {
                    await Task.Delay(50);
                }
            });
        }

        /// <summary>
        /// 停止并清空任务队列
        /// </summary>
        /// <returns>
        /// </returns>
        public async Task ClearJobs()
        {
            await Stop();

            if (!IsFinished)
                Jobs = new ConcurrentQueue<JobDetail>();

            OnLog?.Invoke($"{Environment.NewLine}*************** 指令清空 : 待执行指令:{Statics.Undo} 已完成:{Statics.Done} ***************{Environment.NewLine}", JobSchedulerEventType.任务列表改变);
        }

        /// <summary>
        /// 清空完成任务列表
        /// </summary>
        public void ClearFinishedJobs()
        {
            FinishedJobs = new ConcurrentQueue<JobDetail>();
        }

        /// <summary>
        /// 批量添加任务
        /// </summary>
        /// <param name="initAction">
        /// </param>
        /// <param name="jobDetails">
        /// </param>
        public void AddButch(Action<JobDetail> initAction, params JobDetail[] jobDetails)
        {
            for (int i = 0; i < jobDetails.Length; i++)
            {
                var jd = jobDetails[i];
                initAction(jd);
                Jobs.Enqueue(jd);
            }
            OnLog?.Invoke($"{Environment.NewLine}*************** 指令加载 : 待执行指令:{Statics.Undo} 已完成:{Statics.Done} ***************{Environment.NewLine}", JobSchedulerEventType.任务列表改变);
        }

        #endregion 业务方法
    }

    #region bf

    //public class JobScheduler
    //{
    //    #region 初始化

    // private static readonly Lazy<JobScheduler> lazy = new Lazy<JobScheduler>(() => new
    // JobScheduler());

    // private JobScheduler() { task = new Task(() => { while (!cts.IsCancellationRequested &&
    // Queue.Count > 0) { JobDetail jd; if (!Queue.TryPeek(out jd)) return;

    // while (!cts.IsCancellationRequested && jd.CanExec) { var state = JobState.完成; var value = "";
    // try { value = jd.ExecHandler.Invoke(jd.Command); jd.Command.Response.Value = value; if
    // (jd.CallbackHandler != null) state = jd.CallbackHandler.Invoke(jd.Command.Response); } catch
    // (Exception e) { value = $"Error: {e.Message}"; state = JobState.失败; }

    // jd.Events.Add(new JobEvent(value, state, DateTime.Now)); }

    // if (jd.State == JobState.完成) { Queue.TryDequeue(out jd); FinishedJobs.Add((DateTime.Now,
    // jd)); } } }, cts.Token); }

    // public static JobScheduler Instance => lazy.Value;

    // //private static readonly object locker = new object();

    // /// <summary> /// 任务队列 /// </summary> public ConcurrentQueue<JobDetail> Queue { get; private
    // set; } = new ConcurrentQueue<JobDetail>();

    // /// <summary> /// 完成列表 /// </summary> public ConcurrentBag<(DateTime, JobDetail)>
    // FinishedJobs { get; private set; } = new ConcurrentBag<(DateTime, JobDetail)>();

    // private CancellationTokenSource cts = new CancellationTokenSource();

    // private Task task;

    // #endregion 初始化

    // #region 业务

    // public bool IsFinished => Queue.Count == 0;

    // public TaskStatus TaskStatus => task.Status;

    // public void Start() { if (task.Status == TaskStatus.Running) return; task.Start(); }

    // public void Stop() { cts.Cancel(); }

    //    #endregion 业务
    //}

    #endregion bf
}