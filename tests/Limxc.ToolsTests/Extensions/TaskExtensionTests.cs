using System;
using System.Threading.Tasks;
using FluentAssertions;
using Limxc.Tools.Extensions;
using Xunit;

namespace Limxc.ToolsTests.Extensions;

public class TaskExtensionTests
{
    [Fact]
    public async Task TimeoutAfterTest()
    {
        var f1 = () => Test().TimeoutAfter(1000);
        var f2 = () => Test().TimeoutAfter(3000);
        await f1.Should().ThrowAsync<TimeoutException>();
        await f2.Should().NotThrowAsync();

        var f3 = () => TestRtn().TimeoutAfter(1000);
        var f4 = () => TestRtn().TimeoutAfter(3000);
        await f3.Should().ThrowAsync<TimeoutException>();
        var r = await f4();
        r.Should().Be(1);
    }

    private async Task<int> TestRtn()
    {
        await Task.Delay(2000);
        return 1;
    }

    private async Task Test()
    {
        await Task.Delay(2000);
    }
}