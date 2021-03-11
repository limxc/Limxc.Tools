using System.Collections.Generic;
using FluentAssertions;
using Xunit;

namespace Limxc.Tools.Extensions.Tests
{
    public class CollectionExtensionTests
    {
        [Fact]
        public void SplitTest()
        {
            var str = "1234567";
            str.Split(2).Should().BeEquivalentTo(new[] {'1', '2'}, new[] {'3', '4'}, new[] {'5', '6'});

            str.Split(2, false).Should()
                .BeEquivalentTo(new[] {'1', '2'}, new[] {'3', '4'}, new[] {'5', '6'}, new[] {'7'});
        }

        [Fact]
        public void AddOrUpdateTest()
        {
            ICollection<(int, int)> list = new List<(int, int)> {(1, 1), (2, 2), (3, 3)};
            var dict = new Dictionary<int, int> {{1, 1}, {2, 2}, {3, 3}};

            list.AddOrUpdate((4, 4), p => p.Item1 == 4);
            list.Should().BeEquivalentTo(new List<(int, int)> {(1, 1), (2, 2), (3, 3), (4, 4)});

            list.AddOrUpdate((3, 33), p => p.Item1 == 3);
            list.Should().BeEquivalentTo(new List<(int, int)> {(1, 1), (2, 2), (3, 33), (4, 4)});

            dict.AddOrUpdate(4, 4);
            dict.Should().BeEquivalentTo(new Dictionary<int, int> {{1, 1}, {2, 2}, {3, 3}, {4, 4}});

            dict.AddOrUpdate(4, 5, false);
            dict.Should().BeEquivalentTo(new Dictionary<int, int> {{1, 1}, {2, 2}, {3, 3}, {4, 4}});

            dict.AddOrUpdate(4, 6);
            dict.Should().BeEquivalentTo(new Dictionary<int, int> {{1, 1}, {2, 2}, {3, 3}, {4, 6}});
        }
    }
}