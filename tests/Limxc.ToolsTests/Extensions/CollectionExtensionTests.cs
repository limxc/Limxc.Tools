using System.Collections.Generic;
using FluentAssertions;
using Limxc.Tools.Extensions;
using Xunit;

namespace Limxc.ToolsTests.Extensions;

public class CollectionExtensionTests
{
    [Fact]
    public void SplitTest()
    {
        var str = "1234567";
        str.Split(2)
            .Should()
            .BeEquivalentTo(new[] { new[] { '1', '2' }, new[] { '3', '4' }, new[] { '5', '6' } });

        str.Split(2, false)
            .Should()
            .BeEquivalentTo(
                new[] { new[] { '1', '2' }, new[] { '3', '4' }, new[] { '5', '6' }, new[] { '7' } }
            );
    }

    [Fact]
    public void AddOrUpdateTest()
    {
        ICollection<(int, int)> list = new List<(int, int)> { (1, 1), (2, 2), (3, 3) };
        var dict = new Dictionary<int, int>
        {
            { 1, 1 },
            { 2, 2 },
            { 3, 3 }
        };

        list.AddOrUpdate(p => p.Item1 == 4, (4, 4));
        list.Should().BeEquivalentTo(new List<(int, int)> { (1, 1), (2, 2), (3, 3), (4, 4) });

        list.AddOrUpdate(p => p.Item1 == 3, (3, 33));
        list.Should().BeEquivalentTo(new List<(int, int)> { (1, 1), (2, 2), (3, 33), (4, 4) });

        dict.AddOrUpdate(4, 4);
        dict.Should()
            .BeEquivalentTo(
                new Dictionary<int, int>
                {
                    { 1, 1 },
                    { 2, 2 },
                    { 3, 3 },
                    { 4, 4 }
                }
            );

        dict.AddOrUpdate(4, 5, false);
        dict.Should()
            .BeEquivalentTo(
                new Dictionary<int, int>
                {
                    { 1, 1 },
                    { 2, 2 },
                    { 3, 3 },
                    { 4, 4 }
                }
            );

        dict.AddOrUpdate(4, 6);
        dict.Should()
            .BeEquivalentTo(
                new Dictionary<int, int>
                {
                    { 1, 1 },
                    { 2, 2 },
                    { 3, 3 },
                    { 4, 6 }
                }
            );
    }

    [Fact]
    public void GetOrDefaultTest()
    {
        var dict = new Dictionary<int, int>
        {
            { 1, 1 },
            { 2, 2 },
            { 3, 3 }
        };
        dict.GetOrDefault(5, -1).Should().Be(-1);
        dict.GetOrDefault(5).Should().Be(0);
    }
}
