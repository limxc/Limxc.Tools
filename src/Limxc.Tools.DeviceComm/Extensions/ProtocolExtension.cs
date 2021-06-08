using System;
using System.Collections.Generic;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks;
using Limxc.Tools.DeviceComm.Abstractions;
using Limxc.Tools.Entities.Communication;

namespace Limxc.Tools.DeviceComm.Extensions
{
    public static class ProtocolExtension
    {
        /// <summary>
        ///     解析返回值
        /// </summary>
        /// <param name="protocol"></param>
        /// <param name="bytes"></param>
        /// <param name="waitMs"></param>
        /// <returns></returns>
        public static async Task<byte[]> SendAsync(this IProtocol protocol, byte[] bytes, int waitMs)
        {
            var now = DateTimeOffset.Now;
            if (await protocol.SendAsync(bytes))
                return await protocol.Received.SkipUntil(now).TakeUntil(now.AddMilliseconds(waitMs)).ToTask();
            return new byte[0];
        }

        /// <summary>
        ///     Send解析任务完成通知
        /// </summary>
        /// <param name="protocol"></param>
        /// <param name="context"></param>
        /// <param name="schedulerRunTime">rx处理时间</param>
        /// <returns></returns>
        public static async Task WaitingSendResult(this IProtocol protocol, CommTaskContext context,
            int schedulerRunTime = 100)
        {
            var state = 0; //等待中..

            var dis = protocol.History
                .TakeUntil(DateTimeOffset.Now.AddMilliseconds(context.Timeout + schedulerRunTime))
                .FirstOrDefaultAsync(p => ((CommTaskContext) p).Id == context.Id)
                .ObserveOn(TaskPoolScheduler.Default)
                .Subscribe(p => { state = p == null ? 1 : 2; });

            await protocol.SendAsync(context).ConfigureAwait(false);

            while (state == 0) await Task.Delay(10).ConfigureAwait(false);

            dis.Dispose();

            if (state == 1)
                throw new Exception($"Send Result Lost. {nameof(CommTaskContext.Id)}:{context.Id}");
        }

        /// <summary>
        ///     任务队列执行
        /// </summary>
        /// <param name="protocol"></param>
        /// <param name="queue"></param>
        /// <param name="token"></param>
        /// <param name="schedulerRunTime">rx处理时间</param>
        /// <returns></returns>
        public static async Task ExecQueue(this IProtocol protocol, List<CommTaskContext> queue,
            CancellationToken token,
            int schedulerRunTime = 100)
        {
            foreach (var task in queue)
                while (!token.IsCancellationRequested && task.State != CommContextState.Success &&
                       task.State != CommContextState.NoNeed && task.RemainTimes > 0)
                {
                    task.RemainTimes--;
                    await protocol.WaitingSendResult(task, schedulerRunTime).ConfigureAwait(false);
                }
        }
    }
}