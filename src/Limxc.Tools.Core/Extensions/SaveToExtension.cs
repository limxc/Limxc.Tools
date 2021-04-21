using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Limxc.Tools.Core.Extensions
{
    public static class SaveToExtension
    {
        /// <summary>
        ///     保存至 fullPath
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="fullPath"></param>
        /// <param name="append"></param>
        /// <returns></returns>
        public static async Task SaveTo(this string msg, string fullPath, bool append = true)
        {
            if (string.IsNullOrWhiteSpace(fullPath))
                throw new ArgumentException($"FullPath is invalid. {fullPath}");
            var folder = Path.GetDirectoryName(fullPath);
            if (!Directory.Exists(folder) && !string.IsNullOrWhiteSpace(folder))
                Directory.CreateDirectory(folder);

            using (var fs = new FileStream(fullPath,
                append ? FileMode.Append : FileMode.Create,
                append ? FileAccess.Write : FileAccess.ReadWrite,
                FileShare.ReadWrite,
                4096,
                FileOptions.Asynchronous))
            {
                using (var sw = new StreamWriter(fs, Encoding.UTF8))
                {
                    await sw.WriteAsync(msg.Trim());
                }
            }
        }

        /// <summary>
        ///     保存至 nameof(T)-yyyyMMddHHmmss.json
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="folder"></param>
        /// <param name="ext"></param>
        /// <returns></returns>
        public static async Task SaveTo<T>(this T obj, string folder, string ext = ".json")
        {
            if (string.IsNullOrWhiteSpace(folder))
                throw new ArgumentException($"Folder path is invalid. {folder}");
            if (!Directory.Exists(folder) && !string.IsNullOrWhiteSpace(folder))
                Directory.CreateDirectory(folder);

            var path = Path.Combine(folder, $"{nameof(T)}-{DateTime.Now:yyyyMMddHHmmss}", ext);

            using (var fs = new FileStream(path,
                FileMode.Create,
                FileAccess.ReadWrite,
                FileShare.ReadWrite,
                4096,
                FileOptions.Asynchronous))
            {
                using (var sw = new StreamWriter(fs, Encoding.UTF8))
                {
                    // ReSharper disable once MethodHasAsyncOverload
                    await sw.WriteAsync(JsonConvert.SerializeObject(obj));
                }
            }
        }
    }
}