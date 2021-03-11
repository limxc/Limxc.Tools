﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Limxc.Tools.Common
{
    public class TaskBody<T>
    {
        public TaskBody(Func<CancellationToken, Task<T>> task, int retryCount, string id)
        {
            Task = task;
            RetryCount = retryCount;
            Id = id;
        }

        public Func<CancellationToken, Task<T>> Task { get; }
        public int RetryCount { get; }
        public string Id { get; }
    }

    public class TaskHistory<T>
    {
        public TaskHistory(DateTime execTime, string id, T result, string message)
        {
            ExecTime = execTime;
            Id = id;
            Result = result;
            Message = message;
        }

        public DateTime ExecTime { get; }
        public string Id { get; }
        public T Result { get; }
        public string Message { get; }

        public override string ToString()
        {
            return $"@{ExecTime} {Id}: {Message} | {Result}";
        }
    }

    public class TaskQueue<TRet>
    {
        private readonly Queue<TaskBody<TRet>> _queue;

        public TaskQueue()
        {
            Tasks = new List<TaskBody<TRet>>();
            History = new List<TaskHistory<TRet>>();

            _queue = new Queue<TaskBody<TRet>>();
        }

        /// <summary>Tasks</summary>
        public List<TaskBody<TRet>> Tasks { get; }


        /// <summary>Execution History</summary>
        public List<TaskHistory<TRet>> History { get; }

        /// <summary>Waiting For Execution</summary>
        public IEnumerable<TaskBody<TRet>> PendingQueue => _queue.AsEnumerable();

        private void BuildOnce()
        {
            if (Tasks.Count > 0 && History.Count == 0 && _queue.Count == 0)
                Build();
        }

        /// <summary>Build Queue From Tasks.</summary>
        public void Build()
        {
            History.Clear();
            _queue.Clear();
            Tasks.ForEach(task => _queue.Enqueue(task));
        }

        /// <summary>Clear TaskQueue</summary>
        public void Clear()
        {
            Tasks.Clear();
            History.Clear();
            _queue.Clear();
        }

        /// <summary>
        ///     Add Task
        /// </summary>
        /// <param name="task">Task Func</param>
        /// <param name="retryCount">Execution times = RetryCount + 1</param>
        /// <param name="id">Task Id</param>
        public void Add(Func<CancellationToken, Task<TRet>> task, int retryCount = 0, string id = null)
        {
            Tasks.Add(new TaskBody<TRet>(task, retryCount, id));
        }

        public Task Exec()
        {
            return Exec(CancellationToken.None);
        }

        public Task Exec(double timeoutSeconds)
        {
            return Exec(new CancellationTokenSource((int) (timeoutSeconds * 1000)).Token);
        }

        public async Task Exec(CancellationToken token)
        {
            BuildOnce();
            while (_queue.Count > 0)
            {
                var item = _queue.Peek();

                var remainCount = 1 + (item.RetryCount < 0 ? 0 : item.RetryCount);
                var pass = false;
                var res = default(TRet);
                string error = null;
                while (!pass && remainCount > 0)
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
                        History.Add(new TaskHistory<TRet>(DateTime.Now, item.Id, res,
                            $"RemainCount:{remainCount} State:{pass} {error}"));
                    }

                if (!pass)
                    return;
                _queue.Dequeue();
            }
        }

        /// <summary>
        ///     Combine Queues
        /// </summary>
        /// <param name="queues"></param>
        /// <returns></returns>
        public TaskQueue<TRet> Combine(params TaskQueue<TRet>[] queues)
        {
            foreach (var queue in queues) Tasks.AddRange(queue.Tasks);

            return this;
        }
    }
}