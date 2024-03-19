using System;
using System.Collections.Generic;
using System.Linq;

namespace Limxc.Tools.Extensions
{
    public static class CollectionExtension
    {
        /// <summary>
        ///     拆分子数组
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="size"></param>
        /// <param name="strict">不足组是否抛弃</param>
        /// <returns></returns>
        public static IEnumerable<IEnumerable<T>> Split<T>(
            this IEnumerable<T> source,
            int size,
            bool strict = true
        )
        {
            var array = source as T[] ?? source.ToArray();
            if (strict)
                for (var i = 0; i < array.Count() / size; i++)
                    yield return array.Skip(i * size).Take(size);
            else
                for (var i = 0; i < (float)array.Count() / size; i++)
                    yield return array.Skip(i * size).Take(size);
        }

        public static Dictionary<TKey, TValue> AddOrUpdate<TKey, TValue>(
            this Dictionary<TKey, TValue> source,
            TKey key,
            TValue value,
            bool update = true
        )
        {
            if (!source.ContainsKey(key))
                source.Add(key, value);
            else if (update)
                source[key] = value;

            return source;
        }

        public static TValue GetOrDefault<TKey, TValue>(
            this Dictionary<TKey, TValue> source,
            TKey key,
            TValue defaultValue = default
        )
        {
            if (!source.ContainsKey(key))
                return defaultValue;

            return source[key];
        }

        public static ICollection<T> AddOrUpdate<T>(
            this ICollection<T> source,
            Func<T, bool> condition,
            T value
        )
        {
            var res = source.Where(condition);
            var array = res as T[] ?? res.ToArray();
            if (array.Any())
                foreach (var item in array.ToList())
                    source.Remove(item);

            source.Add(value);

            return source;
        }
    }
}