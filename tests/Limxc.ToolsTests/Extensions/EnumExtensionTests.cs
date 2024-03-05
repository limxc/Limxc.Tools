using System.Collections.Generic;
using System.ComponentModel;
using FluentAssertions;
using Limxc.Tools.Extensions;
using Xunit;

namespace Limxc.ToolsTests.Extensions;

public class EnumExtensionTests
{
    [Fact]
    public void NameTest()
    {
        TestEnum.A.Name().Should().Be("A");
    }

    [Fact]
    public void DescriptionTest()
    {
        TestEnum.A.Description().Should().Be("a");
        TestEnum.C.Description().Should().Be("");
    }

    [Fact]
    public void GetNamesTest()
    {
        default(TestEnum).GetNames().Should().BeEquivalentTo("A", "B", "C");
    }

    [Fact]
    public void GetNameDescriptionsTest()
    {
        default(TestEnum)
            .GetNameDescriptions()
            .Should()
            .BeEquivalentTo(new List<(string, string)> { ("A", "a"), ("B", "b") });

        default(TestEnum)
            .GetNameDescriptions(true)
            .Should()
            .BeEquivalentTo(new List<(string, string)> { ("A", "a"), ("B", "b"), ("C", "") });
    }

    [Fact]
    public void ToEnumTest()
    {
        "B".ToEnum<TestEnum>().Should().Be(TestEnum.B);
        "".ToEnum<TestEnum>().Should().Be(TestEnum.A);
        "b".ToEnum<TestEnum>().Should().Be(TestEnum.A);
    }

    [Fact]
    public void ToEnumFromDescriptionTest()
    {
        "a".ToEnumByDesc<TestEnum>().Should().Be(TestEnum.A);
        "b".ToEnumByDesc<TestEnum>().Should().Be(TestEnum.B);
        "ac".ToEnumByDesc<TestEnum>().Should().Be(TestEnum.A);
    }

    private enum TestEnum
    {
        [Description("a")]
        A,

        [Description("b")]
        B,
        C
    }
}
