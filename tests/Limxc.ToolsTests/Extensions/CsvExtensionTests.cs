using System;
using FluentAssertions;
using Limxc.Tools.Extensions;
using Xunit;

namespace Limxc.ToolsTests.Extensions;

public class CsvExtensionTests
{
    [Fact]
    public void CsvTest()
    {
        var data = new[]
        {
            (1, 3.3, "1-10岁"),
            (10, 9.6, "10以上岁")
        };

        var header = new[] { "Age", "Par", "Desc" };

        var csvStr = data.ToCsv(header);
        var csvData = csvStr.CsvTo<(int, double, string)>();

        data.Should().BeEquivalentTo(csvData);
        csvStr.Should().Contain(string.Join(",", header));

        Action act1 = () => new[]
        {
            (1, 3.3, (3f, true), "1-10岁"),
            (10, 9.6, (9f, false), "10以上岁")
        }.ToCsv();
        act1.Should().Throw<Exception>();


        Action act2 = () => new[]
        {
            (1, 3.3, new A(), "1-10岁"),
            (10, 9.6, new A(), "10以上岁")
        }.ToCsv();
        act2.Should().Throw<Exception>();
    }

    private class A
    {
    }
}