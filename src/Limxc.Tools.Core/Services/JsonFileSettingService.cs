using System;
using System.IO;
using System.Reactive.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;
using Limxc.Tools.Contract.Common;
using Limxc.Tools.Contract.Interfaces;
using Limxc.Tools.Extensions;

namespace Limxc.Tools.Core.Services
{
    public class JsonFileSettingService<T> : ISettingService<T> where T : class, new()
    {
        private readonly IDisposable _disposable;
        private readonly FileSystemWatcher _fileSystemWatcher;
        private readonly JsonSerializerOptions _jsonSerializerOptions;
        private readonly ILogService _logService;

        public JsonFileSettingService(ILogService logService = null)
        {
            _logService = logService;
            _jsonSerializerOptions = new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
                WriteIndented = true,
                IgnoreReadOnlyProperties = true,
                ReferenceHandler = ReferenceHandler.IgnoreCycles
            };
            "".ToJson();
            _fileSystemWatcher =
                new FileSystemWatcher(
                    Path.GetDirectoryName(FullPath) ??
                    throw new InvalidOperationException($"Cant find config folder.({Path.GetDirectoryName(FullPath)})"),
                    FileName)
                {
                    EnableRaisingEvents = true,
                    NotifyFilter = NotifyFilters.LastWrite
                };

            _disposable = Observable.FromEventPattern(_fileSystemWatcher, nameof(FileSystemWatcher.Changed))
                .Throttle(TimeSpan.FromSeconds(0.5))
                .Subscribe(_ =>
                    SettingChanged?.Invoke(Load()));
        }

        public virtual string Name => typeof(T).Name;
        protected virtual string Folder => EnvPath.Default.SettingFolder();
        public string FileName => $"{Name}.Setting.json";

        public string BackUpPath =>
            Path.Combine(Folder, $"{Name}.{DateTime.Now:yyyyMMddHHmmss}.Setting.json");

        public string FullPath => Path.Combine(Folder, FileName);

        public Action<T> SettingChanged { get; set; }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public virtual T Load()
        {
            var setting = new T();
            if (File.Exists(FullPath))
                try
                {
                    var json = FullPath.Load(Encoding.UTF8);
                    setting = JsonSerializer.Deserialize<T>(json, _jsonSerializerOptions);
                }
                catch (Exception)
                {
                    _logService?.Error($"配置文件加载失败,已备份并重置.{BackUpPath}");
                    File.Copy(FullPath, BackUpPath, true);
                    throw;
                }
            else
                Save(setting);

            return setting;
        }

        public virtual void Save(T setting)
        {
            try
            {
                var json = JsonSerializer.Serialize(setting, _jsonSerializerOptions);

                json.Save(FullPath, false, Encoding.UTF8);
            }
            catch (Exception)
            {
                _logService?.Error("配置文件保存失败.");
                throw;
            }
        }

        public virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _disposable?.Dispose();
                _fileSystemWatcher?.Dispose();
            }
        }
    }
}