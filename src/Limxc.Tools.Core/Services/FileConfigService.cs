using System;
using System.IO;
using Limxc.Tools.Core.Abstractions;
using SharpConfig;

namespace Limxc.Tools.Core.Services
{
    public class FileConfigService : IConfigService
    {
        public void Set(string key, string value, string section = "", string fileName = "")
        {
            var fullPath = GetFilePath(fileName);
            if (string.IsNullOrWhiteSpace(section))
                section = "Settings";
            var config = Load(fullPath);
            config[section][key].StringValue = value;
            config.SaveToFile(fileName);
        }

        public string Get(string key, string section = "", string fileName = "")
        {
            var fullPath = GetFilePath(fileName);
            if (string.IsNullOrWhiteSpace(section))
                section = "Settings";
            var config = Load(fullPath);
            if (!config.Contains(section, key))
            {
                Set(key, "", section, fullPath);
                return "";
            }

            return config[section][key].StringValue;
        }

        public void Set<T>(T obj, string fileName = "") where T : class, new()
        {
            var fullPath = GetFilePath(fileName);
            var config = Load(fullPath);
            config.RemoveAllNamed(typeof(T).FullName);
            config.Add(Section.FromObject(typeof(T).FullName, obj));
            config.SaveToFile(fullPath);
        }

        public T Get<T>(string fileName = "") where T : class, new()
        {
            var fullPath = GetFilePath(fileName);
            var config = Load(fullPath);
            if (!config.Contains(typeof(T).FullName))
            {
                var def = new T();
                Set(def, fullPath);
                return def;
            }

            return config[typeof(T).FullName].ToObject<T>();
        }

        public string GetFilePath(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                fileName = "Common.cfg";

            var basePath = Path.Combine(Environment.CurrentDirectory, "Settings");

            var path = Path.Combine(basePath, fileName);
            Directory.CreateDirectory(basePath);
            if (!File.Exists(path))
                File.Create(path).Close();
            return path;
        }

        private Configuration Load(string fullPath)
        {
            using (var fs = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (var sr = new StreamReader(fs))
                {
                    return Configuration.LoadFromString(sr.ReadToEnd());
                }
            }
        }
    }
}