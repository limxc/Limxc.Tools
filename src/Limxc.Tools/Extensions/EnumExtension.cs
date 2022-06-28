using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Limxc.Tools.Extensions
{
    public static class EnumExtension
    {
        public static string Description(this Enum enumValue)
        {
            var value = string.Empty;
            var field = enumValue.GetType().GetField(enumValue.ToString());
            var objs = field.GetCustomAttributes(typeof(DescriptionAttribute), false);
            if (objs.Length == 0)
                return value;

            return ((DescriptionAttribute)objs[0]).Description;
        }

        public static List<string> Names(this Enum e)
        {
            var rst = new List<string>();
            foreach (var item in Enum.GetValues(e.GetType()))
            {
                var name = Enum.GetName(e.GetType(), item);

                rst.Add(name);
            }

            return rst;
        }

        public static string Name(this Enum e)
        {
            return Enum.GetName(e.GetType(), e);
        }

        public static T ToEnum<T>(this string obj) where T : Enum
        {
            if (!Enum.IsDefined(typeof(T), obj))
                return default;
            return (T)Enum.Parse(typeof(T), obj);
        }
    }
}