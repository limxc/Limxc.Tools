using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Limxc.Tools.Extensions
{
    public static class FileExtension
    {
        public static void Save<T>(this T obj, string fullPath)
        {
            var json = obj.ToJson();
            Save(json, fullPath, false);
        }

        public static T Load<T>(this string fullPath)
        {
            if (!File.Exists(fullPath))
                return default;

            try
            {
                return Load(fullPath).JsonTo<T>();
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
            Encoding encoding = null
        )
        {
            if (string.IsNullOrWhiteSpace(fullPath))
                throw new ArgumentException($"FullPath is invalid. {fullPath}");
            var folder = Path.GetDirectoryName(fullPath);
            if (!Directory.Exists(folder) && !string.IsNullOrWhiteSpace(folder))
                Directory.CreateDirectory(folder);

            if (append)
                using (
                    var fs = new FileStream(
                        fullPath,
                        FileMode.Append,
                        FileAccess.Write,
                        FileShare.ReadWrite
                    )
                )
                using (var sr = new StreamWriter(fs, encoding ?? Encoding.Default))
                {
                    sr.Write(msg);
                }
            else
                using (
                    var fs = new FileStream(
                        fullPath,
                        FileMode.OpenOrCreate,
                        FileAccess.ReadWrite,
                        FileShare.ReadWrite
                    )
                )
                {
                    fs.SetLength(0);

                    using (var sr = new StreamWriter(fs, encoding ?? Encoding.Default))
                    {
                        sr.Write(msg);
                    }
                }
        }

        public static async Task SaveAsync(
            this string msg,
            string fullPath,
            bool append = false,
            Encoding encoding = null
        )
        {
            if (string.IsNullOrWhiteSpace(fullPath))
                throw new ArgumentException($"FullPath is invalid. {fullPath}");
            var folder = Path.GetDirectoryName(fullPath);
            if (!Directory.Exists(folder) && !string.IsNullOrWhiteSpace(folder))
                Directory.CreateDirectory(folder);

            if (append)
                using (
                    var fs = new FileStream(
                        fullPath,
                        FileMode.Append,
                        FileAccess.Write,
                        FileShare.ReadWrite,
                        4096,
                        FileOptions.Asynchronous
                    )
                )
                using (var sr = new StreamWriter(fs, encoding ?? Encoding.Default))
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
                        FileShare.ReadWrite,
                        4096,
                        FileOptions.Asynchronous
                    )
                )
                {
                    fs.SetLength(0);

                    using (var sr = new StreamWriter(fs, encoding ?? Encoding.Default))
                    {
                        await sr.WriteAsync(msg);
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
                    FileMode.OpenOrCreate,
                    FileAccess.Read,
                    FileShare.ReadWrite
                )
            )
            using (var sr = new StreamReader(fs, encoding ?? Encoding.Default))
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
                    FileMode.OpenOrCreate,
                    FileAccess.Read,
                    FileShare.ReadWrite,
                    4096,
                    FileOptions.Asynchronous
                )
            )
            {
                using (var sr = new StreamReader(fs, encoding ?? Encoding.Default))
                {
                    var res = await sr.ReadToEndAsync();
                    return res;
                }
            }
        }
    }
}