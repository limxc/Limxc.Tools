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
            var filePath = GetFilePath(fileName);
            if (string.IsNullOrWhiteSpace(section))
                section = "Settings";
            var configuration = Load(filePath);
            configuration[section][key].StringValue = value;
            //configuration.SaveToFile(filePath);

            using (var fileStream =
                new FileStream(filePath, FileMode.Open, FileAccess.Write, FileShare.ReadWrite))
            {
                configuration.SaveToStream(fileStream);
            }
        }

        public string Get(string key, string section = "", string fileName = "")
        {
            var filePath = GetFilePath(fileName);
            if (string.IsNullOrWhiteSpace(section))
                section = "Settings";
            var configuration = Load(filePath);
            if (configuration.Contains(section, key))
                return configuration[section][key].StringValue;
            Set(key, "", section, filePath);
            return "";
        }

        public void Set<T>(T obj, string fileName = "") where T : class, new()
        {
            var filePath = GetFilePath(fileName);
            var configuration = Load(filePath);
            configuration.RemoveAllNamed(typeof(T).FullName);
            configuration.Add(Section.FromObject(typeof(T).FullName, obj));
            //configuration.SaveToFile(filePath);

            using (var fileStream =
                new FileStream(filePath, FileMode.Open, FileAccess.Write, FileShare.ReadWrite))
            {
                configuration.SaveToStream(fileStream);
            }
        }

        public T Get<T>(string fileName = "") where T : class, new()
        {
            var filePath = GetFilePath(fileName);
            var configuration = Load(filePath);
            if (configuration.Contains(typeof(T).FullName))
                return configuration[typeof(T).FullName].ToObject<T>();
            var obj = new T();
            Set(obj, filePath);
            return obj;
        }

        public string GetFilePath(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                fileName = "Common.cfg";
            var str = Path.Combine(Environment.CurrentDirectory, "Settings");
            var path = Path.Combine(str, fileName);
            Directory.CreateDirectory(str);
            if (!File.Exists(path))
                File.Create(path).Close();
            return path;
        }

        private Configuration Load(string fullPath)
        {
            //return Configuration.LoadFromFile(fullPath);

            using (var fileStream =
                new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (var streamReader = new StreamReader(fileStream))
                {
                    return Configuration.LoadFromString(streamReader.ReadToEnd());
                }
            }
        }
    }
}