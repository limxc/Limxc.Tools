using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

// ReSharper disable MethodOverloadWithOptionalParameter

namespace Limxc.Tools.Extensions
{
    public static class FileExtension
    {
        private static readonly Encoding DefaultEncoding = Encoding.UTF8;

        private static Encoding GetEncoding(string fullPath)
        {
            var encoding = DefaultEncoding;

            try
            {
                if (File.Exists(fullPath))
                    encoding = fullPath.GetFileEncoding();
            }
            catch
            {
                // ignored
            }

            return encoding;
        }

        private static async Task<Encoding> GetEncodingAsync(string fullPath)
        {
            var encoding = DefaultEncoding;

            try
            {
                if (File.Exists(fullPath))
                    encoding = await fullPath.GetFileEncodingAsync();
            }
            catch
            {
                // ignored
            }

            return encoding;
        }

        #region Save<T>

        public static void Save<T>(this T obj, string fullPath)
        {
            Save(obj, fullPath, false, false, DefaultEncoding);
        }

        public static Task SaveAsync<T>(this T obj, string fullPath)
        {
            return SaveAsync(obj, fullPath, false, false, DefaultEncoding);
        }

        public static void Save<T>(
            this T obj,
            string fullPath,
            bool append = false,
            bool share = false,
            Encoding encoding = null)
        {
            var json = obj.ToJson();
            Save(json, fullPath, append, share, encoding);
        }

        public static Task SaveAsync<T>(
            this T obj,
            string fullPath,
            bool append = false,
            bool share = false,
            Encoding encoding = null)
        {
            var json = obj.ToJson();
            return SaveAsync(json, fullPath, append, share, encoding);
        }

        #endregion

        #region Load<T>

        public static T Load<T>(this string fullPath)
        {
            return Load<T>(fullPath, GetEncoding(fullPath));
        }

        public static T Load<T>(this string fullPath, Encoding encoding)
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

        public static async Task<T> LoadAsync<T>(this string fullPath)
        {
            return await LoadAsync<T>(fullPath, await GetEncodingAsync(fullPath));
        }

        public static async Task<T> LoadAsync<T>(this string fullPath, Encoding encoding)
        {
            if (!File.Exists(fullPath))
                return default;

            try
            {
                var str = await LoadAsync(fullPath, encoding);
                return str.JsonTo<T>();
            }
            catch
            {
                return default;
            }
        }

        #endregion

        #region Save string

        public static void Save(this string str, string fullPath)
        {
            Save(str, fullPath, false, false, DefaultEncoding);
        }

        public static void Save(
            this string str,
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
                using (var sr = new StreamWriter(fs, encoding ?? DefaultEncoding))
                {
                    sr.Write(str);
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

                    using (var sr = new StreamWriter(fs, encoding ?? DefaultEncoding))
                    {
                        sr.Write(str);
                        sr.Flush();
                    }
                }
        }

        public static Task SaveAsync(this string str, string fullPath)
        {
            return SaveAsync(str, fullPath, false, false, DefaultEncoding);
        }

        public static async Task SaveAsync(
            this string str,
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
                using (var sr = new StreamWriter(fs, encoding ?? DefaultEncoding))
                {
                    await sr.WriteAsync(str);
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

                    using (var sr = new StreamWriter(fs, encoding ?? DefaultEncoding))
                    {
                        await sr.WriteAsync(str);
                        await sr.FlushAsync();
                    }
                }
        }

        #endregion

        #region Load string

        public static string Load(this string fullPath)
        {
            return Load(fullPath, GetEncoding(fullPath));
        }

        public static string Load(this string fullPath, Encoding encoding)
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
            using (var sr = new StreamReader(fs, encoding ?? DefaultEncoding))
            {
                return sr.ReadToEnd();
            }
        }

        public static async Task<string> LoadAsync(this string fullPath)
        {
            return await LoadAsync(fullPath, await GetEncodingAsync(fullPath));
        }

        public static async Task<string> LoadAsync(this string fullPath, Encoding encoding)
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
                using (var sr = new StreamReader(fs, encoding ?? DefaultEncoding))
                {
                    return await sr.ReadToEndAsync();
                }
            }
        }

        #endregion
    }
}