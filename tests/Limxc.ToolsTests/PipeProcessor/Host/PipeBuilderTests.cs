using FluentAssertions;
using Limxc.Tools.Pipeline.Builder;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Limxc.Tools.Pipeline.Host.Tests
{
    public class PipeBuilderTests
    {
        [Fact()]
        public async Task CancelPipeLineTest()
        {
            IPipeBuilder<PipeTestContext> pipe = new PipeBuilder<PipeTestContext>()
                    .Use(c =>
                    {
                        c.Msg += "_1";
                        c.Value += 1;
                    }, "处理1")
                    .Use(async c =>//100ms
                    {
                        await Task.Delay(1000);
                        c.Msg += "_2";
                        c.Value += 2;
                    }, "处理2")
                    .Use((c, t) =>//1100ms
                    {
                        c.Msg += "_3";
#pragma warning disable AsyncFixer02 // Long-running or blocking operations inside an async method
                        Thread.Sleep(1000);
#pragma warning restore AsyncFixer02 // Long-running or blocking operations inside an async method
                        if (t.IsCancellationRequested)
                            return;
                        c.Value += 3;
                    }, "处理3")
                    .Use(async c =>//2100ms
                    {
                        await Task.Delay(1000);
                        c.Msg += "_4";
                        c.Value += 4;
                    })
                    .Build();

            //full
            var rst = await pipe.RunAsync(new PipeTestContext("Test", 0), CancellationToken.None);

            rst.Body.Msg.Should().Be("Test_1_2_3_4");
            rst.Body.Value.Should().Be(10);

            rst.Snapshots.Count.Should().Be(4);

            rst.Snapshots[0].Body.Msg.Should().Be("Test");
            rst.Snapshots[1].Body.Msg.Should().Be("Test_1");
            rst.Snapshots[2].Body.Msg.Should().Be("Test_1_2");
            rst.Snapshots[3].Body.Msg.Should().Be("Test_1_2_3");

            rst.Snapshots[0].Body.Value.Should().Be(0);
            rst.Snapshots[1].Body.Value.Should().Be(1);
            rst.Snapshots[2].Body.Value.Should().Be(3);
            rst.Snapshots[3].Body.Value.Should().Be(6);

            var dt = rst.Snapshots[0].CreateTime;
            rst.Snapshots[1].CreateTime.Should().BeCloseTo(dt, 100);
            rst.Snapshots[2].CreateTime.Should().BeCloseTo(dt.AddSeconds(1), 100);
            rst.Snapshots[3].CreateTime.Should().BeCloseTo(dt.AddSeconds(2), 100);

            //800ms _1_2
            var rst800 = await pipe.RunAsync(new PipeTestContext("Test", 0), new CancellationTokenSource(800).Token);

            var rst800CreateTimes = rst800.Snapshots.Select(p => p.CreateTime);
            (rst800CreateTimes.Max() - rst800CreateTimes.Min()).TotalMilliseconds.Should().BeGreaterThan(800);//进入方法前不足800ms, 执行完毕后超过800ms, 快照间隔也超过了800ms

            rst800.Body.Msg.Should().Be("Test_1_2");
            rst800.Body.Value.Should().Be(3);

            //2100ms _1_2_3(第三步执行中断, 因此快照与数据不符)
            var rst1500 = await pipe.RunAsync(new PipeTestContext("Test", 0), new CancellationTokenSource(1500).Token);

            var rst1500CreateTimes = rst1500.Snapshots.Select(p => p.CreateTime);
            (rst1500CreateTimes.Max() - rst1500CreateTimes.Min()).TotalMilliseconds.Should().BeLessThan(1500);

            rst1500.Body.Msg.Should().Be("Test_1_2_3");
            rst1500.Body.Value.Should().Be(3);//执行中断, 因此不等于6

            Debugger.Break();
        }

        private class PipeTestContext
        {
            public string Msg { get; set; }
            public double Value { get; set; }

            public PipeTestContext(string msg, double value)
            {
                Msg = msg;
            }

            public override string ToString()
            {
                return $"{Msg} | {Value}";
            }
        }
    }
}