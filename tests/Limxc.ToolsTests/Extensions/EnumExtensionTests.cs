using System.ComponentModel;
using FluentAssertions;
using Limxc.Tools.Extensions;
using Xunit;

namespace Limxc.ToolsTests.Extensions
{
    public class EnumExtensionTests
    {
        [Fact]
        public void DescriptionTest()
        {
            TestEnum.A.Description().Should().Be("a");
            TestEnum.B.Description().Should().Be("");
        }

        [Fact]
        public void NamesTest()
        {
            TestEnum.A.Names().Should().BeEquivalentTo("A", "B");
        }

        [Fact]
        public void NameTest()
        {
            TestEnum.A.Name().Should().Be("A");
        }

        [Fact]
        public void ToEnumTest()
        {
            "A".ToEnum<TestEnum>().Should().Be(TestEnum.A);
        }

        private enum TestEnum
        {
            [Description("a")] A,
            B
        }
    }
}