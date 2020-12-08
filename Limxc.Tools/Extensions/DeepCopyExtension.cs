using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Limxc.Tools.Extensions
{
    public static class DeepCopyExtension
    {
        private sealed class DeepCopyExp<TIn, TOut>
        {
            private static readonly Func<TIn, TOut> Cache = GetExp();

            private static Func<TIn, TOut> GetExp()
            {
                ParameterExpression expression = Expression.Parameter(typeof(TIn), "p");
                List<MemberBinding> member = new List<MemberBinding>();

                foreach (var item in typeof(TOut).GetProperties())
                {
                    if (!item.CanWrite)
                        continue;

                    MemberExpression property = Expression.Property(expression, typeof(TIn).GetProperty(item.Name));
                    MemberBinding memberBinding = Expression.Bind(item, property);
                    member.Add(memberBinding);
                }

                MemberInitExpression memberInitExpression =
                    Expression.MemberInit(Expression.New(typeof(TOut)), member.ToArray());
                Expression<Func<TIn, TOut>> lambda =
                    Expression.Lambda<Func<TIn, TOut>>(memberInitExpression, new[] { expression });
                return lambda.Compile();
            }

            public static TOut Copy(TIn tIn)
            {
                return Cache(tIn);
            }
        }

        public static T DeepCopy<T>(this T t)
        {
            return DeepCopyExp<T, T>.Copy(t);
        }

        public static TOut DeepCopy<TIn, TOut>(this TIn tIn)
        {
            return DeepCopyExp<TIn, TOut>.Copy(tIn);
        }
    }
}