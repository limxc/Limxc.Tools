using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;

namespace Limxc.Tools.Common
{
    public class TaskQueue<TRet>
    {
        public List<(Func<CancellationToken, Task<TRet>> Task, int RetryCount, string Id)> Tasks { get; }
        public ObservableCollection<(DateTime ExecTime, string Id, TRet Result, string Msg)> History { get; }

        public TaskQueue()
        {
            Tasks = new List<(Func<CancellationToken, Task<TRet>> Task, int RetryCount, string Id)>();
            History = new ObservableCollection<(DateTime ExecTime, string Id, TRet Result, string Msg)>();
        }

        private Queue<(Func<CancellationToken, Task<TRet>> Task, int RetryCount, string Id)> Build()
        {
            var queue = new Queue<(Func<CancellationToken, Task<TRet>> Run, int RetryCount, string Id)>();
            Tasks.ForEach(task => queue.Enqueue(task));
            return queue;
        }

        /// <summary>
        /// 添加任务
        /// </summary>
        /// <param name="task">成功返回值,失败抛出异常</param>
        /// <param name="retryCount">尝试次数 = 重试次数 + 1</param>
        /// <param name="id">任务标识</param>
        public void Add(Func<CancellationToken, Task<TRet>> task, int retryCount = 0, string id = null) => Tasks.Add((task, retryCount, id));

        public void Clear()
        {
            Tasks.Clear();
            History.Clear();
        }

        public Task Exec() => Exec(CancellationToken.None);

        public Task Exec(double timeout) => Exec(new CancellationTokenSource((int)timeout * 1000).Token);

        public async Task Exec(CancellationToken token)
        {
            History.Clear();
            var queue = Build();
            while (queue.Count > 0)
            {
                var item = queue.Dequeue();

                int remainCount = 1 + (item.RetryCount < 0 ? 0 : item.RetryCount);
                bool pass = false;
                TRet res = default(TRet);
                string error = null;
                while (!pass && remainCount > 0)
                {
                    try
                    {
                        token.ThrowIfCancellationRequested();

                        remainCount--;

                        res = await item.Task(token);
                        pass = true;
                    }
                    catch (Exception ex)
                    {
                        error = $"Error:{ex.Message})";
                        pass = false;
                        throw;
                    }
                    finally
                    {
                        History.Add((DateTime.Now, item.Id, res, $"RemainCount:{remainCount} State:{pass} {error}"));
                    }
                }
                if (!pass)
                    return;
            }
        }

        /// <summary>
        /// 合并任务队列
        /// </summary>
        /// <param name="queues"></param>
        /// <returns></returns>
        public TaskQueue<TRet> Combine(params TaskQueue<TRet>[] queues)
        {
            foreach (var queue in queues)
            {
                Tasks.AddRange(queue.Tasks);
            }
            return this;
        }
    }
}