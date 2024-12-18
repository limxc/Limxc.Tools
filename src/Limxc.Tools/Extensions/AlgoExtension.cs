﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Limxc.Tools.Extensions
{
    public static class AlgoExtension
    {
        public static (T[][] Pack, T[] Remain) LocateToPack<T>(this int[] indexes, T[] source)
        {
            var pack = new List<T[]>();

            T[] remain = { };
            if (indexes.Length == 0 || source.Length == 0)
                return (pack.ToArray(), remain);

            if (indexes.Max() > source.Length - 1 || indexes.Min() < 0)
                throw new ArgumentException(
                    $"{nameof(LocateToPack)}: max index {indexes.Max()} > data length {source.Length}!"
                );

            var ids = indexes.Distinct().ToList();
            ids.Sort();

            for (var i = 1; i < ids.Count; i++)
                pack.Add(source.Skip(indexes[i - 1]).Take(indexes[i] - indexes[i - 1]).ToArray());
            remain = source.Skip(indexes.Last()).ToArray();
            return (pack.ToArray(), remain);
        }

        public static (byte[][] Pack, byte[] Remain) LocateToPack(this byte[] source, byte[] pattern)
        {
            var indexes = source.Locate(pattern);

            var pack = new List<byte[]>();
            byte[] remain = { };
            if (indexes.Length == 0 || source.Length == 0)
                return (pack.ToArray(), remain);

            for (var i = 1; i < indexes.Length; i++)
                pack.Add(source.Skip(indexes[i - 1]).Take(indexes[i] - indexes[i - 1]).ToArray());
            remain = source.Skip(indexes.Last()).ToArray();
            return (pack.ToArray(), remain);
        }

        #region Locate

        /// <summary>
        ///     查找目标值所在位置index
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="pattern"></param>
        /// <returns></returns>
        public static int[] Locate(this byte[] bytes, byte[] pattern)
        {
            var matches = new List<int>();
            for (var i = 0; i < bytes.Length; i++)
                if (pattern[0] == bytes[i] && bytes.Length - i >= pattern.Length)
                {
                    var isMatch = true;
                    for (var j = 1; j < pattern.Length && isMatch; j++)
                        if (bytes[i + j] != pattern[j])
                            isMatch = false;
                    if (isMatch)
                    {
                        matches.Add(i);
                        i += pattern.Length - 1;
                    }
                }

            return matches.ToArray();
        }

        /// <summary>
        ///     查找目标值所在位置index
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="pattern"></param>
        /// <returns></returns>
        public static int[] Locate(this string bytes, string pattern)
        {
            var matches = new List<int>();
            for (var i = 0; i < bytes.Length; i++)
                if (pattern[0] == bytes[i] && bytes.Length - i >= pattern.Length)
                {
                    var ismatch = true;
                    for (var j = 1; j < pattern.Length && ismatch; j++)
                        if (bytes[i + j] != pattern[j])
                            ismatch = false;
                    if (ismatch)
                    {
                        matches.Add(i);
                        i += pattern.Length - 1;
                    }
                }

            return matches.ToArray();
        }

        /// <summary>
        ///     查找目标值所在位置index, 泛型版本要慢3倍左右
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="bytes"></param>
        /// <param name="pattern"></param>
        /// <returns></returns>
        public static int[] Locate<T>(this T[] bytes, T[] pattern)
        {
            var matches = new List<int>();
            for (var i = 0; i < bytes.Length; i++)
                if (pattern[0].Equals(bytes[i]) && bytes.Length - i >= pattern.Length)
                {
                    var isMatch = true;
                    for (var j = 1; j < pattern.Length && isMatch; j++)
                        if (!bytes[i + j].Equals(pattern[j]))
                            isMatch = false;
                    if (isMatch)
                    {
                        matches.Add(i);
                        i += pattern.Length - 1;
                    }
                }

            return matches.ToArray();
        }

        #endregion Locate
    }
}