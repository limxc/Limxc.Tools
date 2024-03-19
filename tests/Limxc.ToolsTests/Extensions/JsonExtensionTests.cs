using FluentAssertions;
using Limxc.Tools.Extensions;
using Xunit;

namespace Limxc.ToolsTests.Extensions;

public class JsonExtensionTests
{
    [Fact]
    public void ToJsonTest()
    {
        var t = new ComplexTestEntity();
        t.InnerArrs = new[] { new TestEntity { FloatValue = 3.1f } };
        t.DoubleValue = 3.3;
        var json = t.ToJson();
        var obj = json.JsonTo<ComplexTestEntity>();
        obj.Should().BeEquivalentTo(t);
    }
}