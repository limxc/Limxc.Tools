using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Limxc.Tools.Extensions.Communication
{
    public static class TemplateExtension
    {
        /// <summary>
        ///     计算模板对应的实际长度
        /// </summary>
        /// <param name="template"></param>
        /// <param name="sepBegin"></param>
        /// <param name="sepEnd"></param>
        /// <returns></returns>
        public static int GetLengthByTemplate(
            this string template,
            char sepBegin = '[',
            char sepEnd = ']'
        )
        {
            template = template.Replace(" ", "");
            var len = 0;
            var matchStarted = false;
            var matches = string.Empty;

            foreach (var c in template)
            {
                if (c == sepBegin)
                    matchStarted = true;

                if (matchStarted)
                    matches += c;
                else
                    len++;

                if (c == sepEnd)
                {
                    var count = Convert.ToInt32(matches.Substring(1, matches.Length - 2));
                    len += count;
                    matchStarted = false;
                    matches = string.Empty;
                }
            }

            return len;
        }

        /// <summary>
        ///     根据模板生成模拟数据
        /// </summary>
        /// <param name="template"></param>
        /// <param name="sepBegin"></param>
        /// <param name="sepEnd"></param>
        /// <param name="options">默认:'0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F'</param>
        /// <returns></returns>
        public static string SimulateByTemplate(
            this string template,
            char sepBegin = '[',
            char sepEnd = ']',
            char[] options = null
        )
        {
            if (string.IsNullOrWhiteSpace(template))
                return string.Empty;

            var keys =
                options
                ?? new[]
                {
                    '0',
                    '1',
                    '2',
                    '3',
                    '4',
                    '5',
                    '6',
                    '7',
                    '8',
                    '9',
                    'A',
                    'B',
                    'C',
                    'D',
                    'E',
                    'F'
                };
            var rnd = new Random(Guid.NewGuid().GetHashCode());

            template = template.Replace(" ", "");

            var sb = new StringBuilder();

            var matchStarted = false;
            var matches = string.Empty;

            for (var i = 0; i < template.Length; i++)
            {
                if (template[i] == sepBegin)
                    matchStarted = true;

                if (!matchStarted)
                {
                    sb.Append(template[i]);
                    continue;
                }

                matches += template[i];

                if (template[i] == sepEnd)
                {
                    var count = Convert.ToInt32(matches.Substring(1, matches.Length - 2));
                    for (var j = 0; j < count; j++)
                        sb.Append(keys[rnd.Next(keys.Length)]);

                    matchStarted = false;
                    matches = string.Empty;
                }
            }

            return sb.ToString();
        }

        /// <summary>
        ///     是否匹配模板
        /// </summary>
        /// <param name="source"></param>
        /// <param name="template"></param>
        /// <param name="restrict">是否严格模式</param>
        /// <param name="sepBegin"></param>
        /// <param name="sepEnd"></param>
        /// <returns></returns>
        public static bool IsTemplateMatch(
            this string source,
            string template,
            bool restrict = false,
            char sepBegin = '[',
            char sepEnd = ']'
        )
        {
            if (string.IsNullOrWhiteSpace(template))
                return string.IsNullOrWhiteSpace(source);

            if (string.IsNullOrWhiteSpace(source))
                return false;

            template = template.Replace(" ", "");
            source = source.Replace(" ", "");

            var pattern = template;
            foreach (Match m in Regex.Matches(template, $@"\{sepBegin}[0-9]+\{sepEnd}"))
            {
                var o = m.Value;
                var n = $"[0-9a-fA-F]{{{Convert.ToInt32(o.Replace("[", "").Replace("]", ""))}}}";
                pattern = pattern.Replace(o, n);
            }

            if (restrict)
                pattern = $"^{pattern}$";

            return Regex.IsMatch(source, pattern, RegexOptions.IgnoreCase);
        }

        /// <summary>
        ///     根据模板获取0~n个匹配
        /// </summary>
        /// <param name="source"></param>
        /// <param name="template"></param>
        /// <param name="sepBegin"></param>
        /// <param name="sepEnd"></param>
        /// <returns></returns>
        public static List<string> TryGetTemplateMatchResults(
            this string source,
            string template,
            char sepBegin = '[',
            char sepEnd = ']'
        )
        {
            template = template.Replace(" ", "");
            source = source.Replace(" ", "");

            var list = new List<string>();

            var pattern = template;
            foreach (Match m in Regex.Matches(template, $@"\{sepBegin}[0-9]+\{sepEnd}"))
            {
                var o = m.Value;
                var n = $"[0-9a-fA-F]{{{Convert.ToInt32(o.Replace("[", "").Replace("]", ""))}}}";
                pattern = pattern.Replace(o, n);
            }

            foreach (Match m in Regex.Matches(source, pattern, RegexOptions.IgnoreCase))
                list.Add(m.Value);
            return list;
        }

        /// <summary>
        ///     根据模板从匹配中获取值<see cref="TryGetTemplateMatchResults" />
        ///     <seealso cref="TemplateParserExtension.TryGetTemplateMatchResult" />
        /// </summary>
        /// <param name="matched"></param>
        /// <param name="template"></param>
        /// <param name="sepBegin"></param>
        /// <param name="sepEnd"></param>
        /// <returns></returns>
        public static List<string> GetMatchValues(
            this string matched,
            string template,
            char sepBegin = '[',
            char sepEnd = ']'
        )
        {
            var values = new List<string>();

            if (string.IsNullOrWhiteSpace(template) || string.IsNullOrWhiteSpace(matched))
                return values;

            template = template.Replace(" ", "");

            var matchStarted = false;
            var matches = string.Empty;
            var idx = 0;
            foreach (var c in template)
            {
                if (c == sepBegin)
                    matchStarted = true;

                if (matchStarted)
                    matches += c;
                else
                    idx++;

                if (c == sepEnd)
                {
                    var count = Convert.ToInt32(matches.Substring(1, matches.Length - 2));
                    values.Add(matched.Substring(idx, count));
                    idx += count;

                    matchStarted = false;
                    matches = string.Empty;
                }
            }

            return values;
        }
    }
}