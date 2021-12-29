using System.Linq;
using FluentAssertions;
using Limxc.Tools.Extensions;
using Xunit;

namespace Limxc.ToolsTests.Extensions
{
    public class LinqExtensionTests
    {
        [Fact]
        public void BatchTest()
        {
            var range = Enumerable.Range(0, 5);
            range.Batch(3).Should().BeEquivalentTo(new[] {new[] {0, 1, 2}, new[] {3, 4}});
            range.Batch(7).Should().BeEquivalentTo(new[] {new[] {0, 1, 2, 3, 4}});

            new int[0].Batch(3).Should().BeEquivalentTo(new[] {new int[0]});
        }

        [Fact]
        public void WindowTest()
        {
            var range = Enumerable.Range(0, 5);
            range.Window(3).Should().BeEquivalentTo(new[] {new[] {0, 1, 2}, new[] {1, 2, 3}, new[] {2, 3, 4}});
            range.Window(6).Should().BeEquivalentTo(new[] {new[] {0, 1, 2, 3, 4}});

            new int[0].Window(3).Should().BeEquivalentTo(new[] {new int[0]});
        }
    }
}