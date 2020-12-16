using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;

namespace Limxc.Tools.Extensions
{
    public static class EnumExtension
    {
        public static string Description(this Enum enumValue)
        {
            string value = string.Empty;
            FieldInfo field = enumValue.GetType().GetField(enumValue.ToString());
            object[] objs = field.GetCustomAttributes(typeof(DescriptionAttribute), false);
            if (objs == null || objs.Length == 0)
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
                return default(T);
            return (T)Enum.Parse(typeof(T), obj);
        }
    }
}