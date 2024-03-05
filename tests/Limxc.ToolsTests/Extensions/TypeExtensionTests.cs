using FluentAssertions;
using Limxc.Tools.Extensions;
using Xunit;

namespace Limxc.ToolsTests.Extensions;

public class TypeExtensionTests
{
    [Fact]
    public void IsInheritedFromTest()
    {
        typeof(BirdZoo).IsInheritedFrom(typeof(Zoo<>)).Should().BeTrue();
        typeof(BirdZoo).IsInheritedFrom(typeof(IZoo<>)).Should().BeTrue();
    }

    private class Bird { }

    private class BirdZoo : Zoo<Bird> { }

    private class Zoo<T> : IZoo<T> { }

    private interface IZoo<T> { }
}
