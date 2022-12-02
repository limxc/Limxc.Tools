using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Limxc.Tools.Extensions.Communication
{
    public static class TemplateExtension
    {
        /// <summary>
        ///     计算指令长度,$n 1-F, 最大15
        /// </summary>
        /// <param name="template"></param>
        /// <param name="sep"></param>
        public static int TemplateLength(this string template, char sep = '$')
        {
            try
            {
                template = template.Replace(" ", "");
                var arr = template.ToCharArray();
                var totalLen = arr.Length;
                for (var i = 0; i < arr.Length - 1; i++)
                    if (arr[i] == sep)
                    {
                        var len = arr[i + 1].ToString().HexToInt();
                        len = len > 1 ? len - 1 : 0;
                        totalLen += len * 2;
                    }

                return totalLen;
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        ///     sep + 1~F(1~15)
        /// </summary>
        /// <param name="template"></param>
        /// <param name="resp"></param>
        /// <param name="sep"></param>
        /// <param name="restrict"></param>
        /// <returns></returns>
        public static bool IsTemplateMatch(this string template, string resp, char sep = '$', bool restrict = true)
        {
            if (string.IsNullOrWhiteSpace(template))
                return string.IsNullOrWhiteSpace(resp);

            if (string.IsNullOrWhiteSpace(resp))
                return false;

            template = template.Replace(" ", "");
            resp = resp.Replace(" ", "");

            var regexStr = string.Empty;

            foreach (var item in template.ToStrArray(2))
                if (item[0] == sep)
                    regexStr += $"[0-9a-fA-F]{{{item[1].ToString().HexToInt() * 2}}}";
                else
                    regexStr += item;

            if (restrict)
                regexStr = $"^{regexStr}$";

            return Regex.IsMatch(resp, regexStr, RegexOptions.IgnoreCase);
        }

        /// <summary>
        ///     sepBegin + 十进制数字 + sepEnd
        /// </summary>
        /// <param name="template"></param>
        /// <param name="resp"></param>
        /// <param name="sepBegin"></param>
        /// <param name="sepEnd"></param>
        /// <param name="restrict"></param>
        /// <returns></returns>
        public static bool IsTemplateMatch(this string template, string resp, char sepBegin, char sepEnd,
            bool restrict = true)
        {
            if (string.IsNullOrWhiteSpace(template))
                return string.IsNullOrWhiteSpace(resp);

            if (string.IsNullOrWhiteSpace(resp))
                return false;

            template = template.Replace(" ", "");
            resp = resp.Replace(" ", "");

            var pattern = template;
            foreach (Match m in Regex.Matches(template, $@"{sepBegin}[0-9]+{sepEnd}"))
            {
                var o = m.Value;
                var n = $"[0-9a-fA-F]{{{Convert.ToInt32(o.Replace("[", "").Replace("]", "")) * 2}}}";
                pattern = pattern.Replace(o, n);
            }

            if (restrict)
                pattern = $"^{pattern}$";

            return Regex.IsMatch(resp, pattern, RegexOptions.IgnoreCase);
        }

        public static string TryGetTemplateMatchResult(this string template, string resp, char sep = '$')
        {
            if (string.IsNullOrWhiteSpace(template) || string.IsNullOrWhiteSpace(resp))
                return string.Empty;

            template = template.Replace(" ", "");
            resp = resp.Replace(" ", "");

            var regexStr = string.Empty;

            foreach (var item in template.ToStrArray(2))
                if (item[0] == sep)
                    regexStr += $"[0-9a-fA-F]{{{item[1].ToString().HexToInt() * 2}}}";
                else
                    regexStr += item;

            return Regex.Match(resp, regexStr, RegexOptions.IgnoreCase).Value;
        }

        public static string SimulateResponse(this string template, char sep = '$')
        {
            if (string.IsNullOrWhiteSpace(template))
                return string.Empty;

            var keys = new List<string>
                { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "A", "B", "C", "D", "E", "F" };
            var rnd = new Random(Guid.NewGuid().GetHashCode());

            template = template.Replace(" ", "");
            var resp = string.Empty;

            foreach (var item in template.ToStrArray(2))
                if (item[0] == sep && int.TryParse(item[1].ToString(), out var len))
                    for (var i = 0; i < len * 2; i++)
                        resp += keys[rnd.Next(keys.Count)];
                else
                    resp += item;

            return resp;
        }

        /// <summary>
        ///     sep + 1~F(1~15)
        /// </summary>
        /// <param name="source"></param>
        /// <param name="template"></param>
        /// <param name="sep"></param>
        /// <returns></returns>
        /// <exception cref="FormatException"></exception>
        public static List<string> GetValues(this string source, string template, char sep = '$')
        {
            var values = new List<string>();

            if (string.IsNullOrWhiteSpace(template) || string.IsNullOrWhiteSpace(source))
                return values;

            if (!template.IsTemplateMatch(source))
                throw new FormatException($"Parse Error: Source[{source}] Template[{template}]");

            source = source.Replace(" ", "");
            template = template.Replace(" ", "");

            var arr = template.ToCharArray();
            var skipLen = 0;

            for (var i = 0; i < arr.Length; i++)
                if (arr[i] == sep && i < arr.Length - 1)
                {
                    var len = arr[i + 1].ToString().HexToInt();
                    var tfv = new string(source.Skip(i + skipLen * 2).Take(len * 2).ToArray());
                    skipLen += len > 1 ? len - 1 : 0;
                    values.Add(tfv);
                }

            return values;
        }

        /// <summary>
        ///     sepBegin + 十进制数字 + sepEnd
        /// </summary>
        /// <param name="source"></param>
        /// <param name="template"></param>
        /// <param name="sepBegin">[</param>
        /// <param name="sepEnd">]</param>
        /// <returns></returns>
        /// <exception cref="FormatException"></exception>
        public static List<string> GetValues(this string source, string template, char sepBegin, char sepEnd)
        {
            var values = new List<string>();

            if (string.IsNullOrWhiteSpace(template) || string.IsNullOrWhiteSpace(source))
                return values;

            if (!template.IsTemplateMatch(source, sepBegin, sepEnd))
                throw new FormatException($"Parse Error: Source[{source}] Template[{template}]");

            source = source.Replace(" ", "");
            template = template.Replace(" ", "");

            var matchStarted = false;
            var matches = string.Empty;
            var matchStartIndex = 0;
            var matchesNumber = 0;
            for (var i = 0; i < template.Length; i++)
            {
                if (template[i] == sepEnd)
                {
                    matchStarted = false;
                    matches.Dump();
                    values.Add(string.Concat(source.Skip(matchStartIndex).Take(Convert.ToInt32(matches) * 2)));
                    matches = string.Empty;
                }

                if (matchStarted)
                    matches += template[i];

                if (template[i] == sepBegin)
                {
                    matchStarted = true;
                    matchStartIndex = i - matchesNumber;
                    matchesNumber++;
                }
            }

            return values;
        }
    }
}