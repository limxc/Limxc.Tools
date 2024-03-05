using System;
using System.Diagnostics;
using System.Threading.Tasks;
using FluentAssertions;
using Limxc.Tools.Common;
using Xunit;

namespace Limxc.ToolsTests.Common;

public class TaskFlowTests
{
    [Fact]
    public async Task ExecTest()
    {
        //arrange
        var tf = new TaskFlow((1, 3));
        tf.Add(
            async (x, y) =>
            {
                var inputs = ((int, int))x;
                await Task.Delay(1);
                return inputs.Item1 + inputs.Item2;
            },
            3,
            "Add"
        );

        tf.Add(
            async (x, y) =>
            {
                var inputs = (int)x;
                await Task.Delay(1);
                return inputs * 4.5d;
            },
            3,
            "Multiply"
        );

        tf.Add(
            async (x, y) =>
            {
                var inputs = (double)x;
                await Task.Delay(1);
                return inputs / 3.6f;
            },
            3,
            "Divide"
        );

        //act
        try
        {
            await tf.Exec();
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
        }

        //assert
        tf.History.Count.Should().Be(3);
        tf.Outputs.Should().BeOfType<double>();
        Convert.ToInt32(tf.Outputs).Should().Be(5);
    }
}
