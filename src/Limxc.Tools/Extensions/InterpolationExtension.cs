using System;
using System.Collections.Generic;
using System.Linq;

namespace Limxc.Tools.Extensions
{
    public static class InterpolationExtension
    {
        /// <summary>
        ///     根据X排序
        /// </summary>
        public static (int X, double Y)[] SortX(this (int X, double Y)[] source)
        {
            var length = source.Length;
            for (var i = 0; i < length - 1; i++)
            for (var j = 0; j < length - i - 1; j++)
                if (source[j].X > source[j + 1].X)
                {
                    (source[j + 1].X, source[j].X) = (source[j].X, source[j + 1].X);
                    (source[j + 1].Y, source[j].Y) = (source[j].Y, source[j + 1].Y);
                }

            return source;
        }

        /// <summary>
        ///     线性插值(自动排序)
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static (int X, double Y)[] Linear(this (int X, double Y)[] source)
        {
            source.SortX();

            var list = new List<(int X, double Y)>();
            list.AddRange(source);
            for (var i = 0; i < source.Length - 1; i++)
            {
                var count = source[i + 1].X - source[i].X;
                if (count > 1)
                {
                    var k = (source[i + 1].Y - source[i].Y) / (source[i + 1].X - source[i].X);
                    var b = source[i + 1].Y - k * source[i + 1].X;

                    for (var j = 1; j < count; j++)
                    {
                        var x = source[i].X + j;
                        list.Add((x, k * x + b));
                    }
                }
            }

            return list.ToArray().SortX();
        }

        /// <summary>
        ///     Cubic Spline 三次样条曲线, 自动填充X轴(自动排序)
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static (int X, double Y)[] Spline(this (int X, double Y)[] source)
        {
            source.SortX();

            var xs = new List<double>();
            var minX = source.Select(p => p.X).Min();
            var maxX = source.Select(p => p.X).Max();
            for (var x = minX; x <= maxX; x++)
                xs.Add(x);

            var ys = SplineInsertPoint(source, xs.ToArray());

            return xs.Select(p => (int)p).ToArray().Zip(ys, (vx, vy) => (vx, vy)).ToArray();
        }

        #region Helpers

        #region Spline (int X, double Y)[]

        /// <summary>
        ///     三次样条插值
        /// </summary>
        /// <param name="points">排序好的数</param>
        /// <param name="xs">需要计算的插值点</param>
        /// <returns>返回计算好的数值</returns>
        private static double[] SplineInsertPoint((int X, double Y)[] points, double[] xs)
        {
            var pointsLength = points.Length;
            var h = new double[pointsLength];
            var f = new double[pointsLength];
            var l = new double[pointsLength];
            var v = new double[pointsLength];
            var g = new double[pointsLength];

            for (var i = 0; i < pointsLength - 1; i++)
            {
                h[i] = points[i + 1].X - points[i].X;
                f[i] = (points[i + 1].Y - points[i].Y) / h[i];
            }

            for (var i = 1; i < pointsLength - 1; i++)
            {
                l[i] = h[i] / (h[i - 1] + h[i]);
                v[i] = h[i - 1] / (h[i - 1] + h[i]);
                g[i] = 3 * (l[i] * f[i - 1] + v[i] * f[i]);
            }

            var b = new double[pointsLength];
            var tem = new double[pointsLength];
            var m = new double[pointsLength];
            var fn = (points[pointsLength - 1].Y - points[pointsLength - 2].Y) /
                     (points[pointsLength - 1].X - points[pointsLength - 2].X);

            b[1] = v[1] / 2;
            for (var i = 2; i < pointsLength - 2; i++) b[i] = v[i] / (2 - b[i - 1] * l[i]);
            tem[1] = g[1] / 2;
            for (var i = 2; i < pointsLength - 1; i++) tem[i] = (g[i] - l[i] * tem[i - 1]) / (2 - l[i] * b[i - 1]);
            m[pointsLength - 2] = tem[pointsLength - 2];
            for (var i = pointsLength - 3; i > 0; i--) m[i] = tem[i] - b[i] * m[i + 1];
            m[0] = 3 * f[0] / 2.0;
            m[pointsLength - 1] = fn;
            var xsLength = xs.Length;
            var insertRes = new double[xsLength];
            for (var i = 0; i < xsLength; i++)
            {
                int j;
                for (j = 0; j < pointsLength; j++)
                    if (xs[i] < points[j].X)
                        break;
                j = j - 1;
                if (j == -1 || j == points.Length - 1)
                {
                    if (j == -1)
                        throw new Exception("插值下边界超出");
                    // ReSharper disable once CompareOfFloatsByEqualityOperator
                    if (j == points.Length - 1 && xs[i] == points[j].X)
                        insertRes[i] = points[j].Y;
                    else
                        throw new Exception("插值下边界超出");
                }
                else
                {
                    var p1 = (xs[i] - points[j + 1].X) / (points[j].X - points[j + 1].X);
                    p1 *= p1;
                    var p2 = (xs[i] - points[j].X) / (points[j + 1].X - points[j].X);
                    p2 *= p2;
                    var p3 = p1 * (1 + 2 * (xs[i] - points[j].X) / (points[j + 1].X - points[j].X)) * points[j].Y +
                             p2 * (1 + 2 * (xs[i] - points[j + 1].X) / (points[j].X - points[j + 1].X)) *
                             points[j + 1].Y;

                    var p4 = p1 * (xs[i] - points[j].X) * m[j] + p2 * (xs[i] - points[j + 1].X) * m[j + 1];
                    p4 += p3;
                    insertRes[i] = p4;
                }
            }

            return insertRes;
        }

        #endregion

        #endregion
    }
}