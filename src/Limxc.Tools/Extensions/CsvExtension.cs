using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Limxc.Tools.Extensions
{
    public static class CsvExtension
    {
        /// <summary>
        ///     最大8列
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fileString"></param>
        /// <param name="hasHeader"></param>
        /// <param name="sep"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        /// <exception cref="InvalidDataException"></exception>
        public static T[] CsvTo<T>(this string fileString, bool hasHeader = true, char sep = ',')
        {
            if (!IsTuple(typeof(T)))
                throw new Exception("T must be Tuple");

            var rst = new List<T>();

            var fieldTypes = typeof(T)
                .GetFields(BindingFlags.Instance | BindingFlags.Public)
                .Select(p => p.FieldType)
                .ToArray();

            var allowedTypes = AllowedTypes();
            if (fieldTypes.Any(p => !allowedTypes.Contains(p)))
                throw new Exception("T is nested Tuple");

            var typeCount = fieldTypes.Length;

            var lines = fileString.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines.Skip(hasHeader ? 1 : 0))
                try
                {
                    if (string.IsNullOrWhiteSpace(line))
                        continue;
                    var col = line.Split(sep);
                    var pars = new object[typeCount];
                    for (var i = 0; i < typeCount; i++) pars[i] = Convert.ChangeType(col[i], fieldTypes[i]);
                    rst.Add((T)Activator.CreateInstance(typeof(T), pars));
                }
                catch (Exception ex)
                {
                    throw new InvalidDataException($"Parse Error({ex.Message}): {line}");
                }

            return rst.ToArray();
        }

        /// <summary>
        ///     最大8列
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="header"></param>
        /// <param name="sep"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static string ToCsv<T>(this T[] source, string[] header = null, string sep = ",")
        {
            if (!IsTuple(typeof(T)))
                throw new Exception("T must be Tuple");

            var sb = new StringBuilder();

            var fieldInfos = typeof(T)
                .GetFields(BindingFlags.Instance | BindingFlags.Public)
                .OrderBy(p => p.Name)
                .ToArray();

            var allowedTypes = AllowedTypes();
            if (fieldInfos.Any(p => !allowedTypes.Contains(p.FieldType)))
                throw new Exception("T is nested Tuple");

            if (header?.Length > 0) sb.AppendLine(string.Join(sep, header));

            for (var i = 0; i < source.Length; i++)
            {
                var obj = source[i];
                sb.AppendLine(string.Join(sep, fieldInfos.Select(p => Format(p.GetValue(obj)))));
            }

            return sb.ToString();
        }

        private static Type[] AllowedTypes()
        {
            return new[]
            {
                typeof(int),
                typeof(long),
                typeof(float),
                typeof(double),
                typeof(decimal),
                typeof(string),
                typeof(bool)
            };
        }

        private static bool IsTuple(Type type)
        {
            return type.IsGenericType &&
                   (
                       type.GetGenericTypeDefinition() == typeof(Tuple<>) ||
                       type.GetGenericTypeDefinition() == typeof(ValueTuple<>) ||
                       (type.GetGenericTypeDefinition().FullName?.StartsWith("System.Tuple`") ?? false) ||
                       (type.GetGenericTypeDefinition().FullName?.StartsWith("System.ValueTuple`") ?? false)
                   );
        }

        private static string Format(object value)
        {
            switch (value)
            {
                case null:
                    return "";
                case DateTime dateTime:
                    return dateTime.ToString("yyyy-MM-dd HH:mm:ss");
                case DateTimeOffset dateTimeOffset:
                    return dateTimeOffset.ToString("yyyy-MM-dd HH:mm:ss");
            }

            if (value.GetType().FullName == "System.DateOnly")
                return ((IFormattable)value).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

            var text = (value.ToString() ?? "").Replace("\"", "\"\"");
            if (text.Length > 0 &&
                (text.Contains(",") || text.Contains("\"") || text.Any(c => c < ' ') || char.IsWhiteSpace(text[0]) ||
                 // ReSharper disable once UseIndexFromEndExpression
                 char.IsWhiteSpace(text[text.Length - 1]))
               )
                text = "\"" + text + "\"";

            return text;
        }
    }
}