using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Limxc.Tools.Common
{
    /// <summary>
    ///     非线程安全
    /// </summary>
    public class TaskFlow
    {
        private readonly Queue<TaskNode> _queue;

        public TaskFlow(object inputs)
        {
            Inputs = inputs;
            _queue = new Queue<TaskNode>();
            Nodes = new List<TaskNode>();
            History = new List<TaskNodeHistory>();
        }

        public object Inputs { get; set; }
        public object Outputs { get; set; }

        /// <summary>All Nodes</summary>
        public List<TaskNode> Nodes { get; }

        /// <summary>Execution History</summary>
        public List<TaskNodeHistory> History { get; }

        /// <summary>Waiting For Execution</summary>
        public IEnumerable<TaskNode> PendingNodes => _queue.AsEnumerable();

        private void BuildOnce()
        {
            if (Nodes.Count > 0 && History.Count == 0 && _queue.Count == 0)
                Build();
        }

        /// <summary>Build Nodes</summary>
        public void Build()
        {
            History.Clear();
            _queue.Clear();
            Nodes.ForEach(task => _queue.Enqueue(task));
        }

        /// <summary>Clear TaskFlow</summary>
        public void Clear()
        {
            Nodes.Clear();
            History.Clear();
            _queue.Clear();
        }

        /// <summary>
        ///     Add Task
        /// </summary>
        /// <param name="task">Task Func</param>
        /// <param name="retryCount">Execution times = RetryCount + 1</param>
        /// <param name="id">Task Id</param>
        public void Add(Func<object, CancellationToken, Task<object>> task, int retryCount = 0, string id = null)
        {
            Nodes.Add(new TaskNode(task, retryCount, id));
        }

        public Task Exec()
        {
            return Exec(CancellationToken.None);
        }

        public Task Exec(double timeoutSeconds)
        {
            return Exec(new CancellationTokenSource((int)(timeoutSeconds * 1000)).Token);
        }

        public async Task Exec(CancellationToken token)
        {
            BuildOnce();
            while (_queue.Count > 0)
            {
                Outputs = null;
                var item = _queue.Peek();

                var remainingAttempts = 1 + (item.RetryCount < 0 ? 0 : item.RetryCount);
                var pass = false;
                string error = null;
                while (!pass && remainingAttempts > 0)
                    try
                    {
                        error = null;
                        token.ThrowIfCancellationRequested();

                        remainingAttempts--;

                        Outputs = await item.Task(Inputs, token);
                        pass = true;
                    }
                    catch (OperationCanceledException)
                    {
                        error = "OperationCanceled";
                        throw;
                    }
                    catch (Exception ex)
                    {
                        error = $"Exception({ex.Message})";
                        pass = false;
                        if (remainingAttempts == 0)
                            throw;
                    }
                    finally
                    {
                        History.Add(new TaskNodeHistory(DateTime.Now, item.Id,
                            $"RemainingAttempts:{remainingAttempts} State:{(pass ? "Success" : error ?? "Exception")} Progress:{Nodes.Count - PendingNodes.Count() + 1}/{Nodes.Count}"
                            , Inputs, Outputs));
                    }

                if (!pass)
                    return;
                _queue.Dequeue();

                if (_queue.Count > 0)
                    Inputs = Outputs;
            }
        }

        /// <summary>
        ///     Combine TaskFlows
        /// </summary>
        /// <param name="queues"></param>
        /// <returns></returns>
        public TaskFlow Combine(params TaskFlow[] queues)
        {
            foreach (var queue in queues) Nodes.AddRange(queue.Nodes);

            return this;
        }
    }

    public class TaskNode
    {
        public TaskNode(Func<object, CancellationToken, Task<object>> task, int retryCount, string id)
        {
            Task = task;
            RetryCount = retryCount;
            Id = id;
        }

        public Func<object, CancellationToken, Task<object>> Task { get; }

        /// <summary>
        ///     Execution times = RetryCount + 1
        /// </summary>
        public int RetryCount { get; }

        public string Id { get; }
    }

    public class TaskNodeHistory
    {
        public TaskNodeHistory(DateTime execTime, string id, string message, object inputs, object outputs)
        {
            ExecTime = execTime;
            Id = id;
            Message = message;
            Inputs = inputs;
            Outputs = outputs;
        }

        public DateTime ExecTime { get; }
        public string Id { get; }
        public object Inputs { get; }
        public object Outputs { get; }
        public string Message { get; }

        public override string ToString()
        {
            return $"@{ExecTime} {Id}: {Message}";
        }
    }
}