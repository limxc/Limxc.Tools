using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Limxc.Tools.Extensions
{
    public static class FileExtension
    {
        public static void Save<T>(this T obj, string fullPath, Encoding encoding = null)
        {
            var json = obj.ToJson();
            Save(json, fullPath, false, false, encoding);
        }

        public static T Load<T>(this string fullPath, Encoding encoding = null)
        {
            if (!File.Exists(fullPath))
                return default;

            try
            {
                return Load(fullPath, encoding).JsonTo<T>();
            }
            catch
            {
                return default;
            }
        }

        public static void Save(
            this string msg,
            string fullPath,
            bool append = false,
            bool share = false,
            Encoding encoding = null
        )
        {
            if (string.IsNullOrWhiteSpace(fullPath))
                throw new ArgumentException($"FullPath is invalid. {fullPath}");
            var folder = Path.GetDirectoryName(fullPath);
            if (!Directory.Exists(folder) && !string.IsNullOrWhiteSpace(folder))
                Directory.CreateDirectory(folder);

            var fileShare = share ? FileShare.ReadWrite : FileShare.Read;
            if (append)
                using (
                    var fs = new FileStream(
                        fullPath,
                        FileMode.Append,
                        FileAccess.Write,
                        fileShare
                    )
                )
                using (var sr = new StreamWriter(fs, encoding ?? Encoding.UTF8))
                {
                    sr.Write(msg);
                    sr.Flush();
                }
            else
                using (
                    var fs = new FileStream(
                        fullPath,
                        FileMode.OpenOrCreate,
                        FileAccess.Write,
                        fileShare
                    )
                )
                {
                    fs.Seek(0, SeekOrigin.Begin);
                    fs.SetLength(0);

                    using (var sr = new StreamWriter(fs, encoding ?? Encoding.UTF8))
                    {
                        sr.Write(msg);
                        sr.Flush();
                    }
                }
        }

        public static async Task SaveAsync(
            this string msg,
            string fullPath,
            bool append = false,
            bool share = false,
            Encoding encoding = null
        )
        {
            if (string.IsNullOrWhiteSpace(fullPath))
                throw new ArgumentException($"FullPath is invalid. {fullPath}");
            var folder = Path.GetDirectoryName(fullPath);
            if (!Directory.Exists(folder) && !string.IsNullOrWhiteSpace(folder))
                Directory.CreateDirectory(folder);

            var fileShare = share ? FileShare.ReadWrite : FileShare.Read;
            if (append)
                using (
                    var fs = new FileStream(
                        fullPath,
                        FileMode.Append,
                        FileAccess.Write,
                        fileShare,
                        4096,
                        FileOptions.Asynchronous
                    )
                )
                using (var sr = new StreamWriter(fs, encoding ?? Encoding.UTF8))
                {
                    await sr.WriteAsync(msg);
                    await sr.FlushAsync();
                }
            else
                using (
                    var fs = new FileStream(
                        fullPath,
                        FileMode.OpenOrCreate,
                        FileAccess.ReadWrite,
                        fileShare,
                        4096,
                        FileOptions.Asynchronous
                    )
                )
                {
                    fs.Seek(0, SeekOrigin.Begin);
                    fs.SetLength(0);

                    using (var sr = new StreamWriter(fs, encoding ?? Encoding.UTF8))
                    {
                        await sr.WriteAsync(msg);
                        await sr.FlushAsync();
                    }
                }
        }

        public static string Load(this string fullPath, Encoding encoding = null)
        {
            if (!File.Exists(fullPath))
                return string.Empty;

            using (
                var fs = new FileStream(
                    fullPath,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.ReadWrite
                )
            )
            using (var sr = new StreamReader(fs, encoding ?? Encoding.UTF8))
            {
                return sr.ReadToEnd();
            }
        }

        public static async Task<string> LoadAsync(this string fullPath, Encoding encoding = null)
        {
            if (!File.Exists(fullPath))
                return string.Empty;

            using (
                var fs = new FileStream(
                    fullPath,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.ReadWrite,
                    4096,
                    FileOptions.Asynchronous
                )
            )
            {
                using (var sr = new StreamReader(fs, encoding ?? Encoding.UTF8))
                {
                    return await sr.ReadToEndAsync();
                }
            }
        }
    }
}