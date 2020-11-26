using FluentAssertions;
using System;
using System.Diagnostics;
using Xunit;

namespace Limxc.Tools.DeviceComm.Contracts.Tests
{
    public class CommCmd_CommRespTests
    {
        [Fact()]
        public void CommCmdTest()
        {
            var cmd = new CommCmd("AA 0A $1 0B $2 0C $3 BB", "");
            cmd.Length.Should().Be(2 + 2 + 2 + 2 + 4 + 2 + 6 + 2);
            cmd.ToCommand(11, 222, 333).Should().Be("AA0A 0B 0B 00DE 0C 00014D BB".Replace(" ", ""));
            Debug.WriteLine(cmd.ToString());
        }

        [Fact]
        public void CommRespTest()
        {
            var cmd = new CommCmd("AA0A$10B$20C$3BB", "AA0A$10B$20C$3BB");
            var intParams = new int[] { 11, 222, 333 };
            cmd.Response.GetIntValues(cmd.ToCommand(intParams)).Should().BeEquivalentTo(intParams);
            cmd.Response.GetStrValues(cmd.ToCommand(intParams)).Should().BeEquivalentTo(new string[] { "0B", "00DE", "00014D" });

            cmd.Response.Value.Should().BeNull();
            cmd.Response.Value = cmd.ToCommand(intParams);
            cmd.Response.GetIntValues().Should().BeEquivalentTo(intParams);
            cmd.Response.GetStrValues().Should().BeEquivalentTo(new string[] { "0B", "00DE", "00014D" });

            var cmd2 = new CommCmd("AA0A$10B$20C$3BB", "AA0A$10B$20C$3BD");
            Assert.Throws(typeof(FormatException), () => cmd2.Response.GetStrValues(cmd2.ToCommand(intParams)));
            cmd2.Response.GetStrValues(cmd2.ToCommand(intParams), false).Should().BeEquivalentTo(new string[] { "0B", "00DE", "00014D" });
        }
    }
}