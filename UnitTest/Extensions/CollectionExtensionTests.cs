using FluentAssertions;
using Xunit;

namespace Limxc.Tools.Extensions.Tests
{
    public class CollectionExtensionTests
    {
        [Fact()]
        public void SplitTest()
        {
            var str = "1234567";
            str.Split(2).Should().BeEquivalentTo(new char[][]
                                                            {
                                                                new char[]{ '1','2'},
                                                                new char[]{ '3','4'},
                                                                new char[]{ '5','6'},
                                                            });

            str.Split(2, false).Should().BeEquivalentTo(new char[][]
                                                             {
                                                                new char[]{ '1','2'},
                                                                new char[]{ '3','4'},
                                                                new char[]{ '5','6'},
                                                                new char[]{ '7'},
                                                            });
        }
    }
}