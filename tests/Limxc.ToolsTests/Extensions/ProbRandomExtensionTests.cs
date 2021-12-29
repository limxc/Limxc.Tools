using FluentAssertions;
using Limxc.Tools.Extensions;
using Xunit;

namespace Limxc.ToolsTests.Extensions
{
    public class ProbRandomExtensionTests
    {
        [Fact]
        public void RandomIndexTest()
        {
            new[] {1}.RandomIndex().Should().Be(0);
            new[] {1d}.RandomIndex().Should().Be(0);
            new[] {1f}.RandomIndex().Should().Be(0);
        }
    }
}