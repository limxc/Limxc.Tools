using FluentAssertions;
using Limxc.Tools.Extensions;
using Xunit;

namespace Limxc.ToolsTests.Extensions;

public class MockExtensionTests
{
    [Fact]
    public void MockItTest()
    {
        var obj = new ComplexTestEntity();
        obj.Mock();

        var obj2 = new ComplexTestEntity();
        obj2.Mock();

        obj.Should().NotBeEquivalentTo(obj2);

        obj.StrValue.Should().NotBeNullOrWhiteSpace();
        obj.BoolValue.Should().NotBeNull();
        obj.IntValue.Should().NotBe(0);
        obj.FloatValue.Should().NotBe(0);
        obj.DoubleValue.Should().NotBe(0);
        obj.DecimalValue.Should().NotBe(0);

        obj.TestEntity.StrValue.Should().NotBeNullOrWhiteSpace();
        obj.TestEntity.BoolValue.Should().NotBeNull();
        obj.TestEntity.IntValue.Should().NotBe(0);
        obj.TestEntity.FloatValue.Should().NotBe(0);
        obj.TestEntity.DoubleValue.Should().NotBe(0);
        obj.TestEntity.DecimalValue.Should().NotBe(0);
    }
}
