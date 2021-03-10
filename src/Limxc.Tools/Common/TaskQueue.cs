using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Limxc.Tools.Common
{
    public class TaskQueue<TRet>
    {
        /// <summary>Tasks</summary>
        public List<(Func<CancellationToken, Task<TRet>> Task, int RetryCount, string Id)> Tasks { get; }

        /// <summary>Execution History</summary>
        public ObservableCollection<(DateTime ExecTime, string Id, TRet Result, string Msg)> History { get; }

        /// <summary>Waiting For Execution</summary>
        public IEnumerable<(Func<CancellationToken, Task<TRet>> Task, int RetryCount, string Id)> PendingQueue => queue.AsEnumerable();

        private Queue<(Func<CancellationToken, Task<TRet>> Task, int RetryCount, string Id)> queue;

        public TaskQueue()
        {
            Tasks = new List<(Func<CancellationToken, Task<TRet>> Task, int RetryCount, string Id)>();
            History = new ObservableCollection<(DateTime ExecTime, string Id, TRet Result, string Msg)>();

            queue = new Queue<(Func<CancellationToken, Task<TRet>> Task, int RetryCount, string Id)>();
        }

        private void BuildOnce()
        {
            if (Tasks.Count > 0 && History.Count == 0 && queue.Count == 0)
                Build();
        }

        /// <summary>Build Queue From Tasks.</summary>
        public void Build()
        {
            History.Clear();
            queue.Clear();
            Tasks.ForEach(task => queue.Enqueue(task));
        }

        /// <summary>Clear TaskQueue</summary>
        public void Clear()
        {
            Tasks.Clear();
            History.Clear();
            queue.Clear();
        }

        /// <summary>
        /// Add Task
        /// </summary>
        /// <param name="task">Task Func</param>
        /// <param name="retryCount">Execution times = RetryCount + 1</param>
        /// <param name="id">Task Id</param>
        public void Add(Func<CancellationToken, Task<TRet>> task, int retryCount = 0, string id = null) => Tasks.Add((task, retryCount, id));

        public Task Exec() => Exec(CancellationToken.None);

        public Task Exec(double timeoutSeconds) => Exec(new CancellationTokenSource((int)(timeoutSeconds * 1000)).Token);

        public async Task Exec(CancellationToken token)
        {
            BuildOnce();
            while (queue.Count > 0)
            {
                var item = queue.Peek();

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
                queue.Dequeue();
            }
        }

        /// <summary>
        /// Combine Queues
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