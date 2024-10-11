using System;
using System.Collections.Generic;
using System.Linq;

// ReSharper disable PossibleMultipleEnumeration

namespace Limxc.Tools.Extensions
{
    /// <summary>
    ///     int, double, float, decimal
    /// </summary>
    public static class NumberExtension
    {
        #region Limit

        public static int Limit(this int value, int min, int max)
        {
            value = value < min ? min : value;
            value = value > max ? max : value;

            return value;
        }

        public static double Limit(this double value, double min, double max, int digits = -1)
        {
            if (double.IsNaN(value))
                return min;

            value = value < min ? min : value;
            value = value > max ? max : value;

            if (digits >= 0)
                value = Math.Round(value, digits, MidpointRounding.AwayFromZero);

            return value;
        }

        public static float Limit(this float value, float min, float max, int digits = -1)
        {
            if (float.IsNaN(value))
                return min;

            value = value < min ? min : value;
            value = value > max ? max : value;

            if (digits >= 0)
                value = (float)Math.Round(value, digits, MidpointRounding.AwayFromZero);

            return value;
        }

        /// <summary>
        ///     数值转换溢出时抛出异常
        /// </summary>
        /// <param name="value"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <param name="digits"></param>
        /// <returns></returns>
        public static decimal Limit(this decimal value, decimal min, decimal max, int digits = -1)
        {
            value = value < min ? min : value;
            value = value > max ? max : value;

            if (digits >= 0)
                value = Math.Round(value, digits, MidpointRounding.AwayFromZero);

            return value;
        }

        #endregion

        #region TryTo

        public static int TryInt(this object value, int defaultValue = 0)
        {
            int.TryParse(value.ToString(), out var result);
            return result == 0 ? defaultValue : result;
        }

        public static double TryDouble(this object value, double defaultValue = 0)
        {
            double.TryParse(value.ToString(), out var result);
            return result == 0 ? defaultValue : result;
        }

        public static float TryFloat(this object value, float defaultValue = 0)
        {
            float.TryParse(value.ToString(), out var result);
            return result == 0 ? defaultValue : result;
        }

        public static decimal TryDecimal(this object value, decimal defaultValue = 0)
        {
            decimal.TryParse(value.ToString(), out var result);
            return result == 0 ? defaultValue : result;
        }

        #endregion

        #region Nearest & NearestBy

        /// <summary>
        ///     匹配数据源中最接近的值
        /// </summary>
        /// <param name="source"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static int Nearest(this IEnumerable<int> source, int value)
        {
            if (!source.Any())
                throw new ArgumentException("source is empty.");
            return source.Select(p => (Value: p, Distance: Math.Abs(p - value))).OrderBy(p => p.Distance)
                .FirstOrDefault().Value;
        }

        /// <summary>
        ///     匹配数据源中最接近的值
        /// </summary>
        /// <param name="source"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static double Nearest(this IEnumerable<double> source, double value)
        {
            if (!source.Any())
                throw new ArgumentException("source is empty.");
            return source.Select(p => (Value: p, Distance: Math.Abs(p - value))).OrderBy(p => p.Distance)
                .FirstOrDefault().Value;
        }

        /// <summary>
        ///     匹配数据源中最接近的值
        /// </summary>
        /// <param name="source"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static float Nearest(this IEnumerable<float> source, float value)
        {
            if (!source.Any())
                throw new ArgumentException("source is empty.");
            return source.Select(p => (Value: p, Distance: Math.Abs(p - value))).OrderBy(p => p.Distance)
                .FirstOrDefault().Value;
        }

        /// <summary>
        ///     匹配数据源中最接近的值
        /// </summary>
        /// <param name="source"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static decimal Nearest(this IEnumerable<decimal> source, decimal value)
        {
            if (!source.Any())
                throw new ArgumentException("source is empty.");
            return source.Select(p => (Value: p, Distance: Math.Abs(p - value))).OrderBy(p => p.Distance)
                .FirstOrDefault().Value;
        }

        /// <summary>
        ///     根据<paramref name="keySelector"></paramref>匹配数据源中最接近的值
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <param name="keySelector"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static TSource NearestBy<TSource>(this IEnumerable<TSource> source, Func<TSource, int> keySelector,
            int value)
        {
            var r = source.Select(keySelector).Nearest(value);
            return source.First(p => keySelector(p) == r);
        }

        /// <summary>
        ///     根据<paramref name="keySelector"></paramref>匹配数据源中最接近的值
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <param name="keySelector"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static TSource NearestBy<TSource>(this IEnumerable<TSource> source, Func<TSource, double> keySelector,
            double value)
        {
            var r = source.Select(keySelector).Nearest(value);
            return source.First(p => Math.Abs(keySelector(p) - r) < 1e-9);
        }

        /// <summary>
        ///     根据<paramref name="keySelector"></paramref>匹配数据源中最接近的值
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <param name="keySelector"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static TSource NearestBy<TSource>(this IEnumerable<TSource> source, Func<TSource, float> keySelector,
            float value)
        {
            var r = source.Select(keySelector).Nearest(value);
            return source.First(p => Math.Abs(keySelector(p) - r) < 1e-9);
        }

        /// <summary>
        ///     根据<paramref name="keySelector"></paramref>匹配数据源中最接近的值
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <param name="keySelector"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static TSource NearestBy<TSource>(this IEnumerable<TSource> source, Func<TSource, decimal> keySelector,
            decimal value)
        {
            var r = source.Select(keySelector).Nearest(value);
            return source.First(p => keySelector(p) == r);
        }

        #endregion

        #region Near & NearBy

        /// <summary>
        ///     向上/向下匹配数据源中最接近的值
        /// </summary>
        /// <param name="source"></param>
        /// <param name="value"></param>
        /// <param name="upward"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static int Near(this IEnumerable<int> source, int value, bool upward = true)
        {
            if (!source.Any())
                throw new ArgumentException("source is empty.");
            source = source.OrderBy(p => p).ToArray();
            if (upward)
            {
                var r = source.First();
                if (source.Any(p => value >= p))
                    r = source.Last(p => value >= p);
                return r;
            }
            else
            {
                var r = source.Last();
                if (source.Any(p => value <= p))
                    r = source.First(p => value <= p);
                return r;
            }
        }

        /// <summary>
        ///     向上/向下匹配数据源中最接近的值
        /// </summary>
        /// <param name="source"></param>
        /// <param name="value"></param>
        /// <param name="upward"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static double Near(this IEnumerable<double> source, double value, bool upward = true)
        {
            if (!source.Any())
                throw new ArgumentException("source is empty.");
            source = source.OrderBy(p => p).ToArray();
            if (upward)
            {
                var r = source.First();
                if (source.Any(p => value >= p))
                    r = source.Last(p => value >= p);
                return r;
            }
            else
            {
                var r = source.Last();
                if (source.Any(p => value <= p))
                    r = source.First(p => value <= p);
                return r;
            }
        }

        /// <summary>
        ///     向上/向下匹配数据源中最接近的值
        /// </summary>
        /// <param name="source"></param>
        /// <param name="value"></param>
        /// <param name="upward"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static float Near(this IEnumerable<float> source, float value, bool upward = true)
        {
            if (!source.Any())
                throw new ArgumentException("source is empty.");
            source = source.OrderBy(p => p).ToArray();
            if (upward)
            {
                var r = source.First();
                if (source.Any(p => value >= p))
                    r = source.Last(p => value >= p);
                return r;
            }
            else
            {
                var r = source.Last();
                if (source.Any(p => value <= p))
                    r = source.First(p => value <= p);
                return r;
            }
        }

        /// <summary>
        ///     向上/向下匹配数据源中最接近的值
        /// </summary>
        /// <param name="source"></param>
        /// <param name="value"></param>
        /// <param name="upward"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static decimal Near(
            this IEnumerable<decimal> source,
            decimal value,
            bool upward = true
        )
        {
            if (!source.Any())
                throw new ArgumentException("source is empty.");
            source = source.OrderBy(p => p).ToArray();
            if (upward)
            {
                var r = source.First();
                if (source.Any(p => value >= p))
                    r = source.Last(p => value >= p);
                return r;
            }
            else
            {
                var r = source.Last();
                if (source.Any(p => value <= p))
                    r = source.First(p => value <= p);
                return r;
            }
        }

        /// <summary>
        ///     根据<paramref name="keySelector"></paramref>向上/向下匹配数据源中最接近的值
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <param name="keySelector"></param>
        /// <param name="value"></param>
        /// <param name="upward"></param>
        /// <returns></returns>
        public static TSource NearBy<TSource>(this IEnumerable<TSource> source, Func<TSource, int> keySelector,
            int value, bool upward = true)
        {
            var r = source.Select(keySelector).Near(value, upward);
            return source.First(p => keySelector(p) == r);
        }

        /// <summary>
        ///     根据<paramref name="keySelector"></paramref>向上/向下匹配数据源中最接近的值
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <param name="keySelector"></param>
        /// <param name="value"></param>
        /// <param name="upward"></param>
        /// <returns></returns>
        public static TSource NearBy<TSource>(this IEnumerable<TSource> source, Func<TSource, double> keySelector,
            double value, bool upward = true)
        {
            var r = source.Select(keySelector).Near(value, upward);
            return source.First(p => Math.Abs(keySelector(p) - r) < 1e-9);
        }

        /// <summary>
        ///     根据<paramref name="keySelector"></paramref>向上/向下匹配数据源中最接近的值
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <param name="keySelector"></param>
        /// <param name="value"></param>
        /// <param name="upward"></param>
        /// <returns></returns>
        public static TSource NearBy<TSource>(this IEnumerable<TSource> source, Func<TSource, float> keySelector,
            float value, bool upward = true)
        {
            var r = source.Select(keySelector).Near(value, upward);
            return source.First(p => Math.Abs(keySelector(p) - r) < 1e-9);
        }

        /// <summary>
        ///     根据<paramref name="keySelector"></paramref>向上/向下匹配数据源中最接近的值
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <param name="keySelector"></param>
        /// <param name="value"></param>
        /// <param name="upward"></param>
        /// <returns></returns>
        public static TSource NearBy<TSource>(this IEnumerable<TSource> source, Func<TSource, decimal> keySelector,
            decimal value, bool upward = true)
        {
            var r = source.Select(keySelector).Near(value, upward);
            return source.First(p => keySelector(p) == r);
        }

        #endregion
    }
}