using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Limxc.Tools.Extensions
{
    public static class MockExtension
    {
        public static void MockIt(this object source)
        {
            var sourceType = source.GetType();
            if (sourceType.IsClass)
                foreach (var prop in sourceType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    var type = prop.PropertyType;
                    if (type == typeof(string) || type.IsValueType)
                    {
                        var value = MockValue(type);
                        prop.SetValue(source, value);
                    }
                    else if (!prop.PropertyType.IsValueType)
                    {
                        var subObj = Activator.CreateInstance(type);
                        MockIt(subObj);
                        prop.SetValue(source, subObj);
                    }
                }

            if (sourceType.IsValueType) source = MockValue(sourceType);
        }

        private static object MockValue(Type type)
        {
            var r = new Random(Guid.NewGuid().GetHashCode());

            if (type.BaseType == typeof(Enum))
            {
                var enums = Enum.GetNames(type);
                return Enum.Parse(type, enums.GetValue(r.Next(0, enums.Length)).ToString());
            }

            if (type == typeof(string))
            {
                var az = Enumerable.Range('a', 'z' - 'a' + 1).Select(i => (char) i).ToArray();
                var cl = new List<char>();
                for (var i = 0; i < 8; i++) cl.Add(az[r.Next(0, az.Length)]);
                return new string(cl.ToArray());
            }

            if (type == typeof(bool) || type == typeof(bool?))
                return r.NextDouble() > 0.5 + 0.001d;

            if (type == typeof(int) || type == typeof(int?))
                return r.Next(100, 10000);

            if (type == typeof(float) || type == typeof(float?))
                return (float) (r.NextDouble() * 101 + 1);

            if (type == typeof(double) || type == typeof(double?))
                return r.NextDouble() * 1000 + 1;

            if (type == typeof(decimal) || type == typeof(decimal?))
                return (decimal) (r.NextDouble() * 101 + 1);

            return null;
        }
    }
}