using FluentAssertions;
using Limxc.Tools.Utils;
using Xunit;

namespace Limxc.ToolsTests.Utils;

public class MemoryMapperTests
{
    [Fact]
    public void CreateTest()
    {
        var te = new TestEntity { FloatValue = 2.1f };
        var mapName = "mmintest";
        using var mapper = new MemoryMapper();
        mapper.Create(mapName, te);
        var ter = MemoryMapper.Read<TestEntity>(mapName);
        ter.Should().BeEquivalentTo(te);
    }
}