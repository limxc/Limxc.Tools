using System;
using System.Diagnostics;
using System.Linq;
using FluentAssertions;
using Limxc.Tools.Common;
using Xunit;

namespace Limxc.ToolsTests.Common;

public class ProbRandomTests
{
    [Fact]
    public void NextIndexTest()
    {
        //arrange
        var probs = new[] { 0.5f, 2f, 97.5f };

        Action create = () => new ProbRandom(Array.Empty<float>());
        create.Should().Throw<ArgumentException>();

        var pr = new ProbRandom(probs);

        var res = new long[probs.Length];
        var count = 100000;

        //act
        for (var i = 0; i < count; i++)
        {
            var idx = pr.NextIndex();
            res[idx]++;
        }

        //assert
        for (var i = 0; i < probs.Length; i++)
        {
            var assert = probs[i] / probs.Sum();
            var actual = res[i] / (float)res.Sum();

            var accuracy = 0.5f / 100; //error<0.5%
            Debug.WriteLine(
                $"assert:{assert} - actual:{actual} = {Math.Abs(assert - actual)} | accuracy:{accuracy}"
            );

            assert.Should().BeApproximately(actual, accuracy);
        }
    }
}
