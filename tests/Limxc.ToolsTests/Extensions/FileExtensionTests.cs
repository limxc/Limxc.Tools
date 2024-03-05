using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using Limxc.Tools.Extensions;
using Xunit;

namespace Limxc.ToolsTests.Extensions;

public class FileExtensionTests
{
    [Fact]
    public void GenericSaveLoadTest()
    {
        var filePath = Path.GetTempFileName();

        var obj = new ComplexTestEntity();
        obj.Mock();
        obj.Save(filePath);
        File.Exists(filePath).Should().BeTrue();

        var load = filePath.Load<ComplexTestEntity>();
        load.StrValue.Should().Be(obj.StrValue);
        load.IntValue.Should().Be(obj.IntValue);
        load.FloatValue.Should().Be(obj.FloatValue);
        load.DoubleValue.Should().Be(obj.DoubleValue);
        load.DecimalValue.Should().Be(obj.DecimalValue);
        load.EnumValue.Should().Be(obj.EnumValue);

        Path.GetTempFileName().Load<ComplexTestEntity>().Should().BeNull();
    }

    [Fact]
    public void SaveLoadTest()
    {
        var filePath = Path.GetTempFileName();

        var dt = DateTime.Now.ToString(CultureInfo.InvariantCulture);
        dt.Save(filePath, false);
        File.Exists(filePath).Should().BeTrue();
        var load = filePath.Load();
        load.Should().Be(dt);

        Path.GetTempFileName().Load().Should().BeNullOrWhiteSpace();
    }

    [Fact]
    public async Task SaveLoadAsyncTest()
    {
        var filePath = Path.GetTempFileName();

        var dt = DateTime.Now.ToString(CultureInfo.InvariantCulture);
        await dt.SaveAsync(filePath);
        File.Exists(filePath).Should().BeTrue();
        var load = await filePath.LoadAsync();
        load.Should().Be(dt);

        (await Path.GetTempFileName().LoadAsync()).Should().BeNullOrWhiteSpace();
    }
}
