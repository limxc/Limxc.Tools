using System;
using System.IO;
using FluentAssertions;
using Limxc.Tools.Core.Services;
using Limxc.Tools.Extensions;
using Limxc.ToolsTests;
using Xunit;

namespace Limxc.Tools.CoreTests.Services;

internal class TestJsonFileSettingService : JsonFileSettingService<ComplexTestEntity>
{
    protected override string Folder => Path.GetTempPath();
}

public class JsonFileSettingServiceTests : IDisposable
{
    private readonly TestJsonFileSettingService _settingService;

    public JsonFileSettingServiceTests()
    {
        _settingService = new TestJsonFileSettingService();
    }

    public void Dispose()
    {
        var path = _settingService.FullPath;
        _settingService?.Dispose();
        File.Delete(path);
    }

    [Fact]
    public void SaveAndLoadTest()
    {
        var test = new ComplexTestEntity();
        test.Mock();
        _settingService.Save(test);
        var loadedDemo = _settingService.Load(false);
        loadedDemo.Should().BeEquivalentTo(test);
    }
}
