using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace Limxc.Tools.Common.Tests
{
    public class TaskQueueTests
    {
        [Fact]
        public async Task ExecTest()
        {
            var que = new TaskQueue<bool>();
            //失败重试
            que.Add(token => Run(1000), 0, "cmd1");
            que.Add(token => Run(500, false), 2, "cmd2");
            que.Add(token => Run(500), 0, "cmd3");
            await que.Exec();
            que.History.Select(p => p.Result).Should().BeEquivalentTo(new[] {true, false, true});

            //固定超时
            que.Clear();
            que.Add(token => Run(1000, true, token), 0, "cmd1");
            que.Add(token => Run(1000, true, token), 0, "cmd2");
            que.Add(token => Run(1000, true, token), 0, "cmd3");
            await Assert.ThrowsAsync(typeof(OperationCanceledException), async () => await que.Exec(1.7));
            que.History.Count(p => p.Result).Should().Be(2);

            //取消任务
            que.Clear();
            que.Add(token => Run(500, true, token), 0, "cmd1");
            que.Add(token => Run(500, true, token), 0, "cmd2");
            que.Add(token => Run(500, true, token), 0, "cmd3");
            que.Add(token => Run(500, true, token), 0, "cmd4");
            que.Add(token => Run(500, true, token), 0, "cmd5");
            que.Add(token => Run(500, true, token), 0, "cmd6");
            var cts = new CancellationTokenSource();
            cts.CancelAfter(2100);
            await Assert.ThrowsAsync(typeof(OperationCanceledException), async () => await que.Exec(cts.Token));
            que.History.Count().Should().Be(5);

            //暂停 继续
            que.Build();
            await Assert.ThrowsAsync(typeof(OperationCanceledException), async () => await que.Exec(1.2));
            que.History.Count(p => p.Result).Should().Be(2);
            que.History.Count(p => !p.Result).Should().Be(1);
            que.PendingQueue.Count().Should().Be(4);
            await que.Exec();
            que.History.Count(p => p.Result).Should().Be(6);
            que.History.Count(p => !p.Result).Should().Be(1);
            que.PendingQueue.Count().Should().Be(0);
        }

        private async Task<bool> Run(int ms, bool rst = true)
        {
            await Task.Delay(ms);
            return rst;
        }

        private async Task<bool> Run(int ms, bool rst, CancellationToken token)
        {
            await Task.Delay(ms / 2);
            token.ThrowIfCancellationRequested();
            await Task.Delay(ms / 2);
            return rst;
        }
    }
}