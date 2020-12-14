using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Limxc.Tools.Extensions
{
    public static class StringExtension
    {
        public static string Join(this IEnumerable<string> strs, string separate = ", ") => string.Join(separate, strs);

        public static bool Contains(this string value, IEnumerable<string> keys, bool ignoreCase = true)
        {
            if (!keys.Any() || string.IsNullOrEmpty(value))
            {
                return false;
            }

            if (ignoreCase)
            {
                return Regex.IsMatch(value, string.Join("|", keys.Select(Regex.Escape)), RegexOptions.IgnoreCase);
            }

            return Regex.IsMatch(value, string.Join("|", keys.Select(Regex.Escape)));
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
            {
                rst.Add(double.Parse(item.Value));
            }
            return rst;
        }
    }
}