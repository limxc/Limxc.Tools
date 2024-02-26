using System;
using System.IO;
using FluentAssertions;
using Limxc.Tools.Core.Services;
using Limxc.Tools.CoreTests.Entities;
using Limxc.Tools.Extensions;
using Xunit;

namespace Limxc.Tools.CoreTests.Services;

internal class TestTomlFileSettingService : TomlFileSettingService<Demo>
{
    protected override string Folder => Path.GetTempPath();
}

public class TomlFileSettingServiceTests : IDisposable
{
    private readonly TestTomlFileSettingService _settingService;

    public TomlFileSettingServiceTests()
    {
        _settingService = new TestTomlFileSettingService();
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
        var demo = new Demo();
        demo.Mock();
        _settingService.Save(demo);
        var loadedDemo = _settingService.Load(false);
        loadedDemo.Should().BeEquivalentTo(demo);
    }
}