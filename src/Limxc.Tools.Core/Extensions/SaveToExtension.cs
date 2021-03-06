﻿using System;
using System.IO;
using Limxc.Tools.Core.Utils;
using Newtonsoft.Json;

namespace Limxc.Tools.Core.Extensions
{
    public static class SaveToExtension
    {
        public static void SaveTo(this string msg, string path, string ext = ".txt")
        {
            path = Path.Combine(EnvPath.OutputDir, path, ext);

            var folder = Path.GetDirectoryName(path);
            if (string.IsNullOrWhiteSpace(folder))
                throw new ArgumentException($"Folder path is invalid. {folder}");
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);
            File.AppendAllText(path, msg.Trim() + Environment.NewLine);
        }

        public static void SaveTo<T>(this T obj, string ext = ".json")
        {
            var path = Path.Combine(EnvPath.OutputDir, $"{nameof(T)}-{DateTime.Now:yyyyMMddHHmmss}", ext);
            var folder = Path.GetDirectoryName(path);
            if (string.IsNullOrWhiteSpace(folder))
                throw new ArgumentException($"Folder path is invalid. {folder}");
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            File.AppendAllText(path, JsonConvert.SerializeObject(obj) + Environment.NewLine);
        }
    }
}