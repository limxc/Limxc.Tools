using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Limxc.Tools.Extensions
{
    public static class EnumExtension
    {
        public static string Name(this Enum enumValue)
        {
            return Enum.GetName(enumValue.GetType(), enumValue);
        }

        public static string Description(this Enum enumValue)
        {
            var value = string.Empty;
            var field = enumValue.GetType().GetField(enumValue.ToString());
            var attributes = field.GetCustomAttributes(typeof(DescriptionAttribute), false);
            if (attributes.Length == 0)
                return value;

            return ((DescriptionAttribute)attributes[0]).Description;
        }

        public static List<string> GetNames(this Enum enumValue)
        {
            var rst = new List<string>();
            foreach (var item in Enum.GetValues(enumValue.GetType()))
            {
                var name = Enum.GetName(enumValue.GetType(), item);

                rst.Add(name);
            }

            return rst;
        }

        public static List<(string Name, string Description)> GetNameDescriptions<T>(
            this T enumValue,
            bool hasEmpty = false
        )
            where T : Enum
        {
            var rst = new List<(string Name, string Description)>();

            foreach (var name in Enum.GetNames(typeof(T)))
            {
                var desc = name.ToEnum<T>().Description();
                if (string.IsNullOrWhiteSpace(desc) && !hasEmpty)
                    continue;
                rst.Add((name, desc));
            }

            return rst;
        }

        public static T ToEnum<T>(this string enumName)
            where T : Enum
        {
            if (!Enum.IsDefined(typeof(T), enumName))
                return default;
            return (T)Enum.Parse(typeof(T), enumName);
        }

        public static T ToEnumByDesc<T>(this string enumDescription)
            where T : Enum
        {
            foreach (var name in Enum.GetNames(typeof(T)))
                if (name.ToEnum<T>().Description() == enumDescription)
                    return (T)Enum.Parse(typeof(T), name);

            return default;
        }
    }
}
