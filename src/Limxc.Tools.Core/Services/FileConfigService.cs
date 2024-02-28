using System.IO;
using Limxc.Tools.Contract.Interfaces;
using Limxc.Tools.Core.Common;
using Limxc.Tools.Core.Dependencies.SharpConfig;

namespace Limxc.Tools.Core.Services
{
    public class FileConfigService : IConfigService
    {
        public void Set(string key, string value, string section = "Common", string fileName = "Setting.ini")
        {
            var filePath = GetFilePath(fileName);
            var configuration = Load(filePath);
            configuration[section][key].StringValue = value;
            Save(filePath, configuration);
        }

        public string Get(string key, string def = "", string section = "Common", string fileName = "Setting.ini")
        {
            var filePath = GetFilePath(fileName);
            var configuration = Load(filePath);
            if (configuration.Contains(section, key))
                return configuration[section][key].StringValue;

            Set(key, def, section, filePath);
            return def;
        }

        public void Set<T>(T obj, string fileName = "Setting.ini") where T : class, new()
        {
            var filePath = GetFilePath(fileName);
            var configuration = Load(filePath);
            configuration.RemoveAllNamed(typeof(T).FullName);
            configuration.Add(Section.FromObject(typeof(T).FullName, obj));
            Save(filePath, configuration);
        }

        public T Get<T>(string fileName = "Setting.ini", T def = default) where T : class, new()
        {
            var filePath = GetFilePath(fileName);
            var configuration = Load(filePath);
            if (configuration.Contains(typeof(T).FullName))
                return configuration[typeof(T).FullName].ToObject<T>();

            if (def != default)
                Set(def, filePath);
            return def;
        }

        public string GetFilePath(string fileName)
        {
            return Path.Combine(EnvPath.Default.SettingFolder(), fileName);
        }


        private Configuration Load(string fullPath)
        {
            //return Configuration.LoadFromFile(fullPath);

            using (var fs = new FileStream(fullPath, FileMode.OpenOrCreate, FileAccess.Read, FileShare.ReadWrite))
            {
                return Configuration.LoadFromStream(fs);
            }
        }

        private void Save(string fullPath, Configuration configuration)
        {
            //configuration.SaveToFile(filePath);

            using (var fs = new FileStream(fullPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                fs.SetLength(0);
                configuration.SaveToStream(fs);
            }
        }
    }
}