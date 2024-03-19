using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Limxc.Tools.Extensions.Communication;
using Xunit;

namespace Limxc.ToolsTests.Extensions.Communication;

public class TemplateParserExtensionTests
{
    [Fact]
    public async Task TryGetTemplateMatchResultTest()
    {
        var observable = Observable.Create<string>(async o =>
        {
            foreach (var b in new[] { "AA", "AA", "01", "BB", "BB", "AA", "02", "BB", "CC" })
            {
                await Task.Delay(100);
                o.OnNext(b);
            }

            o.OnCompleted();
            return Disposable.Empty;
        });

        var begin = DateTime.Now;
        var rst = await observable.TryGetTemplateMatchResult("AA[2]BB", 1000);
        rst.Should().Be("AA01BB");
        (DateTime.Now - begin).TotalMilliseconds.Should().BeLessThan(1000);

        var act = async () => await observable.TryGetTemplateMatchResult("AA[4]CC", 500);
        await act.Should().ThrowAsync<TimeoutException>();
    }
}