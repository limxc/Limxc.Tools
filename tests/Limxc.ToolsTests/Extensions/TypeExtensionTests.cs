using System;
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

    [Fact]
    public void ConvertToTest()
    {
        // string->double
        "12.3".ConvertTo<double>().Should().Be(12.3);
        "abc".ConvertTo<double>(0).Should().Be(0);
        var func = () => "abc".ConvertTo<double>();
        func.Should().Throw<Exception>();

        //double->string
        12.3.ConvertTo<string>().Should().Be("12.3");

        //class->string 
        Bird bird = null;
        // ReSharper disable once ExpressionIsAlwaysNull
        bird.ConvertTo<string>().Should().Be("");

        // nullable
        "11".ConvertTo<int?>().Should().Be(11);
        bird.ConvertTo<int?>().Should().Be(null);
        "abc".ConvertTo<int?>(0).Should().Be(0);

        BirdZoo birdZoo = null;
        birdZoo.ConvertTo<Bird>().Should().Be(null);

        birdZoo = new BirdZoo();
        birdZoo.ConvertTo<Bird>(default).Should().Be(null);
        var func2 = () => birdZoo.ConvertTo<Bird>();
        func2.Should().Throw<InvalidCastException>();
    }

    private class Bird
    {
    }

    private class BirdZoo : Zoo<Bird>
    {
    }

    private class Zoo<T> : IZoo<T>
    {
    }

    private interface IZoo<T>
    {
    }
}