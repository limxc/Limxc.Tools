using Limxc.Tools.Core.Utils;
using Newtonsoft.Json;
using System;
using System.IO;

namespace Limxc.Tools.Core.Extensions
{
    public static class SaveToExtension
    {
        public static void SaveTo(this string msg, string path, string ext = ".txt")
        {
            try
            {
                path = Path.Combine(EnvPath.OutputDir, path, ext);

                var folder = Path.GetDirectoryName(path);
                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);
                File.AppendAllText(path, msg.Trim() + Environment.NewLine);
            }
            catch { }
        }

        public static void SaveTo<T>(this T obj, string ext = ".json")
        {
            try
            {
                var path = Path.Combine(EnvPath.OutputDir, $"{nameof(T)}-{DateTime.Now:yyyyMMddHHmmss}", ext);
                var folder = Path.GetDirectoryName(path);
                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                File.AppendAllText(path, JsonConvert.SerializeObject(obj) + Environment.NewLine);
            }
            catch { }
        }
    }
}