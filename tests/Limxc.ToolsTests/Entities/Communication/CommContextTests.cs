using System;
using System.Linq;
using FluentAssertions;
using Limxc.Tools.Entities.Communication;
using Limxc.Tools.Extensions.Communication;
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
            var ctx = new CommContext("AA0A$10B$20C$3BB", "AA0A$10B$20C$3BB", 1000);

            ctx.Response.Value.Should().BeNull();

            var strPars = new[] { "0B", "00DE", "00014D" };
            var intPars = strPars.Select(p => p.HexToInt()).ToArray();

            ctx.Response.Value = ctx.Command.Build(intPars);

            ctx.Response.GetIntValues().Should().BeEquivalentTo(intPars);
            ctx.Response.GetStrValues().Should().BeEquivalentTo(strPars);

            var resp = new CommResponse("AA0A$10B$20C$3BB") { Value = "AA000000BB" };
            Assert.Throws(typeof(FormatException), () => resp.GetStrValues());
        }
    }
}