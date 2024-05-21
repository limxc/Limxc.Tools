using System;
using System.Threading.Tasks;

namespace Limxc.Tools.Extensions
{
    public static class TaskExtension
    {
        public static async Task TimeoutAfter(this Task task, int millisecondsTimeout)
        {
            if (task == await Task.WhenAny(task, Task.Delay(millisecondsTimeout)))
                await task;
            else
                throw new TimeoutException();
        }

        public static async Task<TResult> TimeoutAfter<TResult>(this Task<TResult> task, int millisecondsTimeout)
        {
            if (task == await Task.WhenAny(task, Task.Delay(millisecondsTimeout)))
                return await task;
            throw new TimeoutException();
        }
    }
}