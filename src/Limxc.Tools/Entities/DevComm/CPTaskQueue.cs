using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Limxc.Tools.Entities.DevComm
{
    public class CPTaskQueue
    {
        public List<(Func<CancellationToken, Task<bool>> Run, int RetryCount, string Id)> Tasks { get; }
        public List<(DateTime ExecTime, string Id, bool State, string Msg)> History { get; }

        public CPTaskQueue()
        {
            Tasks = new List<(Func<CancellationToken, Task<bool>> Run, int RetryCount, string Id)>();
            History = new List<(DateTime ExecTime, string Id, bool State, string Msg)>();
        }

        private Queue<(Func<CancellationToken, Task<bool>> Run, int RetryCount, string Id)> ToQueue()
        {
            var queue = new Queue<(Func<CancellationToken, Task<bool>> Run, int RetryCount, string Id)>();
            Tasks.ForEach(t => queue.Enqueue(t));
            return queue;
        }

        public void Add(Func<CancellationToken, Task<bool>> task, int retryCount = 0, string id = null) => Tasks.Add((task, retryCount, id));

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
            var queue = ToQueue();
            while (queue.Count > 0)
            {
                var item = queue.Dequeue();

                int tryExecCount = 1 + (item.RetryCount < 0 ? 0 : item.RetryCount);
                bool pass = false;
                while (!pass && tryExecCount > 0)
                {
                    token.ThrowIfCancellationRequested();

                    tryExecCount--;
                    pass = await item.Run(token);
                    History.Add((DateTime.Now, item.Id, pass, $"{(string.IsNullOrWhiteSpace(item.Id) ? string.Empty : item.Id.Trim() + " ")}执行{(pass ? "成功" : $"失败, 剩余次数:{tryExecCount}")}"));
                }
                if (!pass)
                    return;
            }
        }
    }
}