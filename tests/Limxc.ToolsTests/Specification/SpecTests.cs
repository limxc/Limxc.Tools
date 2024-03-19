using System.Linq;
using FluentAssertions;
using Limxc.Tools.Specification;
using Xunit;

namespace Limxc.ToolsTests.Specification;

public class SpecTests
{
    [Fact]
    public void SpecTest()
    {
        var m = new[] { new(1), new Model(4), new Model(7) };
        var g3 = new Gte3Spec();
        var l5 = new Spec<Model>(p => p.Value <= 5);

        m.Count(p => g3.IsSatisfiedBy(p)).Should().Be(2);
        m.Count(p => l5.IsSatisfiedBy(p)).Should().Be(2);

        m.Count(p => l5.And(g3).IsSatisfiedBy(p)).Should().Be(1);
        m.Count(p => l5.Or(g3).IsSatisfiedBy(p)).Should().Be(3);

        m.Count(p => l5.And(g3).Not().IsSatisfiedBy(p)).Should().Be(2);
        m.Count(p => l5.Or(g3).Not().IsSatisfiedBy(p)).Should().Be(0);

        m.Count(p => l5.Not().IsSatisfiedBy(p)).Should().Be(1);
    }

    private class Gte3Spec : Spec<Model>
    {
        public Gte3Spec()
            : base(p => p.Value >= 3)
        {
        }
    }

    private class Model
    {
        public Model(int value)
        {
            Value = value;
        }

        public int Value { get; }
    }
}