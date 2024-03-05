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
    }
}
