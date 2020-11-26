using Xunit;
using Limxc.Tools.Extensions;
using System;
using System.Collections.Generic;
using System.Text;
using FluentAssertions;

namespace Limxc.Tools.Extensions.Tests
{
    public class DeepCopyExtensionTests
    {
        [Fact()]
        public void DeepCopyTest()
        {
            var tc1 = new TestClass1() 
            { 
                IntValue = 11, 
                BoolValue = true, 
                DoubleValue = 22.2, 
                StrValue = "abc" 
            };

            tc1.DeepCopy().Should().NotBeSameAs(tc1);
            tc1.DeepCopy().Should().BeEquivalentTo(tc1);

            var tc2 = new TestClass2()
            {
                IntValue = 111,
                BoolValue = true,
                StrValue = "abcde",
                FloatValue = 22.2f,
                Class1 = tc1
            };
            tc2.DeepCopy().Should().NotBeSameAs(tc2);
            tc2.DeepCopy().Should().BeEquivalentTo(tc2);
            tc2.Class1.DeepCopy().Should().BeEquivalentTo(tc2.Class1);
             
            tc2.DeepCopy<TestClass2, TestClass1>().Should().BeEquivalentTo(new TestClass1() 
            {
                IntValue = 111,
                BoolValue = true,
                StrValue = "abcde"
            });
        }

        private class TestClass1
        {
            public int IntValue { get; set; }
            public string StrValue { get; set; }
            public bool BoolValue { get; set; }
            public double DoubleValue { get; set; }
        }
        private class TestClass2:TestClass1
        {
            public float FloatValue { get; set; }
            public TestClass1 Class1 { get; set; }
        }
    }
}