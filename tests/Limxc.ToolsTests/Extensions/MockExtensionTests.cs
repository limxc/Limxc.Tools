using FluentAssertions;
using Xunit;

namespace Limxc.Tools.Extensions
{
    public class MockExtensionTests
    {
        public enum Foo
        {
            zoo,
            keep
        }

        [Fact]
        public void MockItTest()
        {
            var obj = new OuterTestObj();
            obj.MockIt();

            var obj2 = new OuterTestObj();
            obj2.MockIt();

            obj.Should().NotBeEquivalentTo(obj2);

            obj.StrValue.Should().NotBeNullOrWhiteSpace();
            obj.BoolValue.Should().NotBeNull();
            obj.IntValue.Should().NotBe(0);
            obj.FloatValue.Should().NotBe(0);
            obj.DoubleValue.Should().NotBe(0);
            obj.DecimalValue.Should().NotBe(0);

            obj.TestObj.StrValue.Should().NotBeNullOrWhiteSpace();
            obj.TestObj.BoolValue.Should().NotBeNull();
            obj.TestObj.IntValue.Should().NotBe(0);
            obj.TestObj.FloatValue.Should().NotBe(0);
            obj.TestObj.DoubleValue.Should().NotBe(0);
            obj.TestObj.DecimalValue.Should().NotBe(0);
        }

        public class TestObj
        {
            public Foo EnumValue { get; set; }
            public string StrValue { get; set; }
            public bool? BoolValue { get; set; }
            public int IntValue { get; set; }
            public float FloatValue { get; set; }
            public double? DoubleValue { get; set; }
            public decimal DecimalValue { get; set; }
        }

        public class OuterTestObj : TestObj
        {
            public TestObj TestObj { get; set; }
        }
    }
}