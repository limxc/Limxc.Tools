using FluentAssertions;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Limxc.Tools.Entities.DevComm.Tests
{
    public class CPTaskQueueTests
    {
        [Fact()]
        public async Task ExecTest()
        {
            var que = new CPTaskQueue();
            //失败重试
            que.Add(token => Run(1000), 0, "cmd1");
            que.Add(token => Run(500, false), 2, "cmd2");
            que.Add(token => Run(500), 0, "cmd3");
            await que.Exec();
            que.History.Select(p => p.State).Should().BeEquivalentTo(new bool[] { true, false, false, false });

            //固定超时
            que.Clear();
            que.Add(token => Run(1000, true, token), 0, "cmd1");
            que.Add(token => Run(1000, true, token), 0, "cmd2");
            que.Add(token => Run(1000, true, token), 0, "cmd3");
            await Assert.ThrowsAsync(typeof(OperationCanceledException), async () => await que.Exec(1.9));
            que.History.Count().Should().Be(1);

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
            que.History.Count().Should().Be(4);
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