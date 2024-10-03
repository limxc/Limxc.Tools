using System;
using System.Linq;

namespace Limxc.Tools.Extensions
{
    public static class TypeExtension
    {
        /// <summary>
        ///     是否实现了泛型接口或继承自泛型基类
        /// </summary>
        /// <param name="type"></param>
        /// <param name="generic"></param>
        public static bool IsInheritedFrom(this Type type, Type generic)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));
            if (generic == null)
                throw new ArgumentNullException(nameof(generic));

            // interfaces
            var check = type.GetInterfaces().Any(IsGenericTypeOf);
            if (check)
                return true;

            // base
            while (type != null && type != typeof(object))
            {
                check = IsGenericTypeOf(type);
                if (check)
                    return true;
                type = type.BaseType;
            }

            return false;

            bool IsGenericTypeOf(Type test)
            {
                return generic == (test.IsGenericType ? test.GetGenericTypeDefinition() : test);
            }
        }

        /// <summary>
        ///     基础数据类型转换器
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="def"></param>
        /// <returns></returns>
        public static T ConvertTo<T>(this object obj, T def)
        {
            try
            {
                // 如果传入对象是 null 或 DBNull，并且目标类型是 string，返回空字符串
                if ((obj == null || obj == DBNull.Value) && typeof(T) == typeof(string))
                    return (T)Convert.ChangeType("", typeof(T));

                // 如果传入对象已经是目标类型，直接返回
                if (obj is T t) return t;

                // 处理 Nullable<T> 类型
                if (typeof(T).IsGenericType)
                {
                    var genericTypeDefinition = typeof(T).GetGenericTypeDefinition();
                    if (genericTypeDefinition == typeof(Nullable<>))
                        // ReSharper disable once AssignNullToNotNullAttribute
                        return (T)Convert.ChangeType(obj, Nullable.GetUnderlyingType(typeof(T)));
                }

                // 进行类型转换
                return (T)Convert.ChangeType(obj, typeof(T));
            }
            catch (InvalidCastException)
            {
                try
                {
                    // 如果发生转换异常，尝试返回原对象的强制转换
                    return (T)obj;
                }
                catch
                {
                    return def;
                }
            }
            catch
            {
                return def;
            }
        }

        /// <summary>
        ///     基础数据类型转换器
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static T ConvertTo<T>(this object obj)
        {
            try
            {
                // 如果传入对象是 null 或 DBNull，并且目标类型是 string，返回空字符串
                if ((obj == null || obj == DBNull.Value) && typeof(T) == typeof(string))
                    return (T)Convert.ChangeType("", typeof(T));

                // 如果传入对象已经是目标类型，直接返回
                if (obj is T t) return t;

                // 处理 Nullable<T> 类型
                if (typeof(T).IsGenericType)
                {
                    var genericTypeDefinition = typeof(T).GetGenericTypeDefinition();
                    if (genericTypeDefinition == typeof(Nullable<>))
                        // ReSharper disable once AssignNullToNotNullAttribute
                        return (T)Convert.ChangeType(obj, Nullable.GetUnderlyingType(typeof(T)));
                }

                // 进行类型转换
                return (T)Convert.ChangeType(obj, typeof(T));
            }
            catch (InvalidCastException)
            {
                // 如果发生转换异常，尝试返回原对象的强制转换
                return (T)obj;
            }
        }
    }
}