using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

// ReSharper disable PossibleMultipleEnumeration

namespace Limxc.Tools.Extensions
{
    /// <summary>
    ///     统计学相关 todo: 需要时继续补充
    /// </summary>
    public static class StatisticsExtension
    {
        #region 均值

        /// <summary>
        ///     均值
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static double Mean(this IEnumerable<double> source)
        {
            if (source == null)
                return 0;
            var values = source as double[] ?? source.ToArray();
            return values.Length == 0 ? 0 : values.Average();
        }

        #endregion

        #region 中位数

        /// <summary>
        ///     中位数
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static double Median(this IEnumerable<double> source)
        {
            var len = source?.Count() ?? 0;
            if (len == 0)
                return 0;

            Debug.Assert(source != null, nameof(source) + " != null");

            var o = source.OrderBy(p => p).ToList();
            if (o.Count() % 2 == 0)
                return (o[o.Count() / 2] + o[o.Count() / 2 - 1]) / 2f;
            return o[(o.Count() - 1) / 2];
        }

        #endregion

        #region 众数

        /// <summary>
        ///     众数
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static double[] Mode(this IEnumerable<double> source)
        {
            var len = source?.Count() ?? 0;
            if (len == 0)
                return Array.Empty<double>();

            Debug.Assert(source != null, nameof(source) + " != null");

            var g = source.GroupBy(p => p).Select(p => (Count: p.Count(), Value: p.Key)).ToList();

            return g.Where(p => p.Count == g.Max(d => d.Count)).Select(p => p.Value).ToArray();
        }

        #endregion

        #region 方差

        /// <summary>
        ///     总体方差
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static double Variance(this IEnumerable<double> source)
        {
            if (source == null)
                return 0;
            var values = source as double[] ?? source.ToArray();
            var mean = values.Mean();

            double variance = 0;

            foreach (var value in values)
                variance += Math.Pow(value - mean, 2);

            return variance / values.Length;
        }

        /// <summary>
        ///     样本方差
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static double SampleVariance(this IEnumerable<double> source)
        {
            if (source == null)
                return 0;
            var values = source as double[] ?? source.ToArray();
            var mean = values.Mean();

            double variance = 0;

            foreach (var value in values)
                variance += Math.Pow(value - mean, 2);

            return variance / (values.Length - 1);
        }

        #endregion

        #region 标准差

        /// <summary>
        ///     标准差
        /// </summary>
        /// <param name="variance">方差</param>
        /// <returns></returns>
        public static double StandardDeviation(this double variance)
        {
            return Math.Sqrt(variance);
        }

        /// <summary>
        ///     标准差
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static double StandardDeviation(this IEnumerable<double> source)
        {
            return Math.Sqrt(source.Variance());
        }

        #endregion
    }
}
