using System;
using System.IO;
using System.Reactive.Linq;
using Limxc.Tools.Contract.Interfaces;
using Limxc.Tools.Core.Common;

namespace Limxc.Tools.Core.Services
{
    public class TomlFileSettingService<T> : ISettingService<T> where T : class, new()
    {
        private readonly IDisposable _disposable;
        private readonly FileSystemWatcher _fileSystemWatcher;
        private readonly ILogService _logService;

        public TomlFileSettingService(ILogService logService = null)
        {
            _logService = logService;

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

        /// <summary>
        ///     文件名前缀
        /// </summary>
        public virtual string Name => typeof(T).Name;

        protected virtual string Folder => EnvPath.Default.SettingFolder();

        /// <summary>
        ///     文件名
        /// </summary>
        public string FileName => $"{Name}.Setting.toml";

        public string BackUpPath =>
            Path.Combine(Folder, $"{Name}.{DateTime.Now:yyyyMMddHHmmss}.Setting.toml");

        public string FullPath => Path.Combine(Folder, FileName);

        public Action<T> SettingChanged { get; set; }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public virtual void Save(T setting)
        {
            try
            {
                //todo
            }
            catch (Exception)
            {
                _logService?.Error("配置文件保存失败.");
                throw;
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="initOnFailure">失败时重建(初始值)</param>
        /// <returns></returns>
        public virtual T Load(bool initOnFailure = true)
        {
            var setting = new T();
            if (File.Exists(FullPath))
                try
                {
                    //todo setting =  
                }
                catch (Exception)
                {
                    _logService?.Error($"配置文件加载失败,已备份并重置.{BackUpPath}");
                    File.Copy(FullPath, BackUpPath, true);
                    if (initOnFailure)
                        Save(new T());
                    throw;
                }
            else
                Save(setting);

            return setting;
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