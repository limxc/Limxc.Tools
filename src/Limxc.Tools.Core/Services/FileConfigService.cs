using Limxc.Tools.Abstractions;
using SharpConfig;
using System;
using System.IO;

namespace Limxc.Tools.Core.Services
{
    public class FileConfigService : IConfigService
    {
        private string FilePath(string fileName)
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

        public void Set(string key, string value, string section = "", string fileName = "")
        {
            var fullPath = FilePath(fileName);
            if (string.IsNullOrWhiteSpace(section))
                section = "Settings";
            var config = Configuration.LoadFromFile(fullPath);
            config[section][key].StringValue = value;
            config.SaveToFile(fileName);
        }

        public string Get(string key, string section = "", string fileName = "")
        {
            var fullPath = FilePath(fileName);
            if (string.IsNullOrWhiteSpace(section))
                section = "Settings";
            var config = Configuration.LoadFromFile(fullPath);
            if (!config.Contains(section, key))
            {
                Set(key, "", section, fullPath);
                return "";
            }
            else
            {
                return config[section][key].StringValue;
            }
        }

        public void Set<T>(T obj, string fileName = "") where T : class, new()
        {
            var fullPath = FilePath(fileName);
            var config = Configuration.LoadFromFile(fullPath);
            config.RemoveAllNamed(typeof(T).FullName);
            config.Add(Section.FromObject(typeof(T).FullName, obj));
            config.SaveToFile(fullPath);
        }

        public T Get<T>(string fileName = "") where T : class, new()
        {
            var fullPath = FilePath(fileName);
            var config = Configuration.LoadFromFile(fullPath);
            if (!config.Contains(typeof(T).FullName))
            {
                var def = new T();
                Set<T>(def, fullPath);
                return def;
            }
            else
            {
                return config[typeof(T).FullName].ToObject<T>();
            }
        }
    }
}