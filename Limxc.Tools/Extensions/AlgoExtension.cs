using System.Collections.Generic;

namespace Limxc.Tools.Extensions
{
    public static class AlgoExtension
    {
        public static int[] Locate(this byte[] bytes, byte[] pattern)
        {
            var matches = new List<int>();
            for (int i = 0; i < bytes.Length; i++)
            {
                if (pattern[0] == bytes[i] && bytes.Length - i >= pattern.Length)
                {
                    bool ismatch = true;
                    for (int j = 1; j < pattern.Length && ismatch; j++)
                    {
                        if (bytes[i + j] != pattern[j])
                            ismatch = false;
                    }
                    if (ismatch)
                    {
                        matches.Add(i);
                        i += pattern.Length - 1;
                    }
                }
            }
            return matches.ToArray();
        }

        public static int[] Locate(this string bytes, string pattern)
        {
            var matches = new List<int>();
            for (int i = 0; i < bytes.Length; i++)
            {
                if (pattern[0] == bytes[i] && bytes.Length - i >= pattern.Length)
                {
                    bool ismatch = true;
                    for (int j = 1; j < pattern.Length && ismatch; j++)
                    {
                        if (bytes[i + j] != pattern[j])
                            ismatch = false;
                    }
                    if (ismatch)
                    {
                        matches.Add(i);
                        i += pattern.Length - 1;
                    }
                }
            }
            return matches.ToArray();
        }

        /// <summary>
        /// 泛型版本要慢3倍左右
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="bytes"></param>
        /// <param name="pattern"></param>
        /// <returns></returns>
        public static int[] Locate<T>(this T[] bytes, T[] pattern)
        {
            var matches = new List<int>();
            for (int i = 0; i < bytes.Length; i++)
            {
                if (pattern[0].Equals(bytes[i]) && bytes.Length - i >= pattern.Length)
                {
                    bool ismatch = true;
                    for (int j = 1; j < pattern.Length && ismatch; j++)
                    {
                        if (!bytes[i + j].Equals(pattern[j]))
                            ismatch = false;
                    }
                    if (ismatch)
                    {
                        matches.Add(i);
                        i += pattern.Length - 1;
                    }
                }
            }
            return matches.ToArray();
        }
    }
}