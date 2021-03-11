using System;
using FluentAssertions;
using Limxc.Tools.Entities.Communication;
using Xunit;

namespace Limxc.ToolsTests.Entities.Communication
{
    public class CommContextTests
    {
        [Fact]
        public void CommandTest()
        {
            var context = new CommContext("AA 0A $1 0B $2 0C $3 BB");
            context.Command.Length.Should().Be(2 + 2 + 2 + 2 + 4 + 2 + 6 + 2);
            var cmd = context.Command.Build(11, 222, 333);
            cmd.Should().Be("AA0A 0B 0B 00DE 0C 00014D BB".Replace(" ", ""));
        }

        [Fact]
        public void ResponseTest()
        {
            var ctx1 = new CommContext("AA0A$10B$20C$3BB", "AA0A$10B$20C$3BB", 1000);
            var intParams = new[] {11, 222, 333};
            ctx1.Response.GetIntValues(ctx1.Command.Build(intParams)).Should().BeEquivalentTo(intParams);
            ctx1.Response.GetStrValues(ctx1.Command.Build(intParams)).Should().BeEquivalentTo("0B", "00DE", "00014D");

            ctx1.Response.Value.Should().BeNull();
            ctx1.Response.Value = ctx1.Command.Build(intParams);
            ctx1.Response.GetIntValues().Should().BeEquivalentTo(intParams);
            ctx1.Response.GetStrValues().Should().BeEquivalentTo("0B", "00DE", "00014D");

            var ctx2 = new CommContext("AA0A$10B$20C$3BB", "AA0A$10B$20C$3f", 1000);
            Assert.Throws(typeof(FormatException), () => ctx2.Response.GetStrValues(ctx2.Command.Build(intParams)));
            var values = ctx2.Response.GetStrValues(ctx2.Command.Build(intParams), false);
            values.Should().BeEquivalentTo("0B", "00DE", "00014D");
        }
    }
}