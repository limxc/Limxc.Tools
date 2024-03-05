using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Limxc.Tools.Extensions
{
    public static class MockExtension
    {
        /// <summary>
        ///     基本类型:Enum,string,bool,int,float,double,decimal,datetime
        ///     列表:Array,List<>
        /// </summary>
        /// <param name="source"></param>
        /// <param name="innerCount">列表填充数</param>
        public static void Mock(this object source, int innerCount = 2)
        {
            var sourceType = source.GetType();

            if (sourceType.IsClass)
                foreach (
                    var prop in sourceType.GetProperties(
                        BindingFlags.Public | BindingFlags.Instance
                    )
                )
                {
                    var type = prop.PropertyType;

                    if (type == typeof(string) || type.IsValueType)
                    {
                        prop.SetValue(source, MockValue(type));
                    }
                    else if (typeof(IEnumerable).IsAssignableFrom(type))
                    {
                        if (type.IsArray)
                        {
                            var subType = type.GetElementType();

                            if (subType != null)
                            {
                                var arr = Array.CreateInstance(subType, innerCount);
                                for (var i = 0; i < innerCount; i++)
                                {
                                    var tmp = Activator.CreateInstance(subType);
                                    Mock(tmp);
                                    arr.SetValue(tmp, i);
                                }

                                prop.SetValue(source, arr);
                            }
                        }

                        if (type.IsGenericType)
                        {
                            var subType = type.GenericTypeArguments.FirstOrDefault();

                            if (subType != null)
                            {
                                var arr = Array.CreateInstance(subType, innerCount);
                                for (var i = 0; i < innerCount; i++)
                                {
                                    var tmp = Activator.CreateInstance(subType);
                                    Mock(tmp);
                                    arr.SetValue(tmp, i);
                                }

                                var list = Activator.CreateInstance(type, arr);
                                prop.SetValue(source, list);
                            }
                        }
                    }
                    else if (!type.IsValueType)
                    {
                        var obj = Activator.CreateInstance(type);
                        Mock(obj);
                        prop.SetValue(source, obj);
                    }
                }
            else if (sourceType.IsValueType)
                MockValue(sourceType);
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
                var az = Enumerable.Range('a', 'z' - 'a' + 1).Select(i => (char)i).ToArray();
                var cl = new List<char>();
                for (var i = 0; i < 8; i++)
                    cl.Add(az[r.Next(0, az.Length)]);
                return new string(cl.ToArray());
            }

            if (type == typeof(bool) || type == typeof(bool?))
                return r.NextDouble() > 0.5;

            if (type == typeof(int) || type == typeof(int?))
                return r.Next(1, 10000);

            if (type == typeof(float) || type == typeof(float?))
                return (float)(r.NextDouble() * 10 + 1);

            if (type == typeof(double) || type == typeof(double?))
                return r.NextDouble() * 100 + 1;

            if (type == typeof(decimal) || type == typeof(decimal?))
                return (decimal)(r.NextDouble() * 1000 + 1);

            if (type == typeof(DateTime) || type == typeof(DateTime?))
                return new DateTime(
                    1900 + r.Next(0, DateTime.Now.Year - 1900 - 1),
                    r.Next(1, 12),
                    r.Next(1, 28)
                );

            return null;
        }
    }
}
