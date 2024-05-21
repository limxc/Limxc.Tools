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
        t.InnerArrs = new[]
        {
            new TestEntity
            {
                FloatValue = 3.1f, DoubleValue = 2.2d, DecimalValue = 3.6m, Bytes = new byte[] { 0xaa, 0xbb }
            }
        };
        t.DoubleValue = 3.3;
        var json = t.ToJson();
        var obj = json.JsonTo<ComplexTestEntity>();
        obj.Should().BeEquivalentTo(t);
    }
}