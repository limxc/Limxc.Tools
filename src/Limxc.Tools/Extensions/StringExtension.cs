using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Limxc.Tools.Extensions
{
    public static class StringExtension
    {
        public static string Join(this IEnumerable<string> strs, string separate = ", ")
        {
            return string.Join(separate, strs);
        }

        public static bool Contains(
            this string value,
            IEnumerable<string> keys,
            bool ignoreCase = true
        )
        {
            var array = keys as string[] ?? keys.ToArray();
            if (!array.Any() || string.IsNullOrEmpty(value))
                return false;

            if (ignoreCase)
                return Regex.IsMatch(
                    value,
                    string.Join("|", array.Select(Regex.Escape)),
                    RegexOptions.IgnoreCase
                );

            return Regex.IsMatch(value, string.Join("|", array.Select(Regex.Escape)));
        }

        public static string DeleteChineseWord(this string str)
        {
            return Regex.Replace(str, @"[\u4e00-\u9fa5]+", "");
        }

        public static string RetainChineseWord(this string str)
        {
            return Regex.Replace(str, @"[^\u4e00-\u9fa5]+", "");
        }

        public static List<double> Numbers(this string str)
        {
            var rst = new List<double>();
            var reg = Regex.Matches(str, @"\d+(.\d+)?", RegexOptions.Compiled);
            foreach (Match item in reg)
                rst.Add(double.Parse(item.Value));
            return rst;
        }

        public static double TryDouble(this string str, double def = 0)
        {
            return double.TryParse(str, out var result) ? result : def;
        }

        public static int TryInt(this string str, int def = 0)
        {
            return int.TryParse(str, out var result) ? result : def;
        }

        public static float TryFloat(this string str, float def = 0)
        {
            return float.TryParse(str, out var result) ? result : def;
        }

        public static bool TryBool(this string str, bool def = false)
        {
            return bool.TryParse(str, out var result) ? result : def;
        }

        public static DateTime? TryDateTime(this string str)
        {
            return DateTime.TryParse(str, out var result) ? result : default(DateTime?);
        }

        public static DateTime TryDateTime(this string str, DateTime def)
        {
            return DateTime.TryParse(str, out var result) ? result : def;
        }

        /// <summary>
        ///     "\u4E2D\u6587" => "中文"
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string Unescape(this string str)
        {
            return Regex.Unescape(str);
        }
    }
}