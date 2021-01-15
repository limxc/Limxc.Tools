using FluentAssertions;
using System;
using System.Diagnostics;
using Xunit;

namespace Limxc.Tools.Entities.DevComm.Tests
{
    public class CPContextTests
    {
        [Fact()]
        public void CommandTest()
        {
            var context = new CPContext("AA 0A $1 0B $2 0C $3 BB", "");
            context.Command.Length.Should().Be(2 + 2 + 2 + 2 + 4 + 2 + 6 + 2);
            context.Command.Build(11, 222, 333).Should().Be("AA0A 0B 0B 00DE 0C 00014D BB".Replace(" ", ""));
            Debug.WriteLine(context);
        }

        [Fact]
        public void ResponseTest()
        {
            var ctx1 = new CPContext("AA0A$10B$20C$3BB", "AA0A$10B$20C$3BB", 1000);
            var intParams = new int[] { 11, 222, 333 };
            ctx1.Response.GetIntValues(ctx1.Command.Build(intParams)).Should().BeEquivalentTo(intParams);
            ctx1.Response.GetStrValues(ctx1.Command.Build(intParams)).Should().BeEquivalentTo(new string[] { "0B", "00DE", "00014D" });

            ctx1.Response.Value.Should().BeNull();
            ctx1.Response.Value = ctx1.Command.Build(intParams);
            ctx1.Response.GetIntValues().Should().BeEquivalentTo(intParams);
            ctx1.Response.GetStrValues().Should().BeEquivalentTo(new string[] { "0B", "00DE", "00014D" });

            var ctx2 = new CPContext("AA0A$10B$20C$3BB", "AA0A$10B$20C$3f", 1000);
            Assert.Throws(typeof(FormatException), () => ctx2.Response.GetStrValues(ctx2.Command.Build(intParams)));
            ctx2.Response.GetStrValues(ctx2.Command.Build(intParams), false).Should().BeEquivalentTo(new string[] { "0B", "00DE", "00014D" });
        }
    }
}