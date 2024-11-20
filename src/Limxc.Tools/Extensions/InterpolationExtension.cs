using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

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
        ///     重采样
        /// </summary>
        /// <param name="source"></param>
        /// <param name="xKeySelector">x</param>
        /// <param name="interpolation">插值方法</param>
        /// <returns></returns>
        public static TSource[] Resample<TSource, TKey>(this TSource[] source,
            Expression<Func<TSource, TKey>> xKeySelector, Func<(int X, double Y)[], (int X, double Y)[]> interpolation)
        {
            source = source.OrderBy(xKeySelector.Compile()).ToArray();

            var member = ((MemberExpression)xKeySelector.Body).Member;
            if (member.MemberType != MemberTypes.Property)
                throw new InvalidExpressionException($"{nameof(xKeySelector)} must be Property.");

            var propertyInfoX = (PropertyInfo)member;

            var propertyInfos = typeof(TSource)
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(p => p.PropertyType == typeof(float) || p.PropertyType == typeof(double) ||
                            p.PropertyType == typeof(decimal))
                .OrderBy(p => p != propertyInfoX)
                .ToArray();

            var type = typeof(TSource);
            (int, double) t = default;

            var param = Expression.Parameter(type, "p");
            var xExp = Expression.Property(param, propertyInfoX.Name);

            var rst = new List<TSource>();
            foreach (var propertyInfoY in propertyInfos)
            {
                var yExp = Expression.Property(param, propertyInfoY.Name);
                var con = t.GetType().GetConstructor(new[] { typeof(int), typeof(double) });
                Debug.Assert(con != null, "(int, double) Constructor != null");

                var interpolationExp = Expression.Lambda<Func<TSource, (int, double)>>(
                        Expression.New(
                            con,
                            xExp,
                            yExp
                        ),
                        param)
                    .Compile();

                var data = interpolation(source.Select(interpolationExp).ToArray());

                for (var i = 0; i < data.Length; i++)
                {
                    var valueX = Expression.Constant(data[i].X);
                    var pre = Expression.Lambda<Predicate<TSource>>(Expression.Equal(xExp, valueX), param)
                        .Compile();

                    if (!rst.Exists(pre))
                    {
                        var r = (TSource)Activator.CreateInstance(type);
                        propertyInfoX.SetValue(r, data[i].X);
                        propertyInfoY.SetValue(r, data[i].Y);
                        rst.Add(r);
                    }
                    else
                    {
                        var exp = Expression.Lambda<Func<TSource, bool>>(Expression.Equal(xExp, valueX), param)
                            .Compile();
                        var r = rst.First(exp);
                        propertyInfoY.SetValue(r, data[i].Y);
                    }
                }
            }

            return rst.ToArray();
        }

        /// <summary>
        ///     线性插值(自动排序)
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static (int X, double Y)[] LinearResample(this (int X, double Y)[] source)
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
        ///     Cubic Spline 三次样条插值(自动排序)
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="Exception"></exception>
        public static (int X, double Y)[] SplineResample(this (int X, double Y)[] source)
        {
            source.SortX();

            if (source == null || source.Length < 2) throw new ArgumentException($"{nameof(source)} Length must >= 2");

            var minX = source.Select(p => p.X).Min();
            var maxX = source.Select(p => p.X).Max();

            var c = source.Length;
            source = new[] { (2 * source[0].X - source[1].X, 2 * source[0].Y - source[1].Y) }
                .Concat(source)
                .Concat(new[] { (2 * source[c - 1].X - source[c - 2].X, 2 * source[c - 1].Y - source[c - 2].Y) })
                .ToArray();

            var xs = new List<double>();
            var tmpMinX = source.Select(p => p.X).Min();
            var tmpMaxX = source.Select(p => p.X).Max();
            for (var x = tmpMinX; x <= tmpMaxX; x++)
                xs.Add(x);

            var pointsLength = source.Length;
            var h = new double[pointsLength];
            var f = new double[pointsLength];
            var l = new double[pointsLength];
            var v = new double[pointsLength];
            var g = new double[pointsLength];

            for (var i = 0; i < pointsLength - 1; i++)
            {
                h[i] = source[i + 1].X - source[i].X;
                f[i] = (source[i + 1].Y - source[i].Y) / h[i];
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
            var fn =
                (source[pointsLength - 1].Y - source[pointsLength - 2].Y)
                / (source[pointsLength - 1].X - source[pointsLength - 2].X);

            b[1] = v[1] / 2;
            for (var i = 2; i < pointsLength - 2; i++)
                b[i] = v[i] / (2 - b[i - 1] * l[i]);
            tem[1] = g[1] / 2;
            for (var i = 2; i < pointsLength - 1; i++)
                tem[i] = (g[i] - l[i] * tem[i - 1]) / (2 - l[i] * b[i - 1]);
            m[pointsLength - 2] = tem[pointsLength - 2];
            for (var i = pointsLength - 3; i > 0; i--)
                m[i] = tem[i] - b[i] * m[i + 1];
            m[0] = 3 * f[0] / 2.0;
            m[pointsLength - 1] = fn;
            var xsLength = xs.Count;
            var insertRes = new double[xsLength];
            for (var i = 0; i < xsLength; i++)
            {
                int j;
                for (j = 0; j < pointsLength; j++)
                    if (xs[i] < source[j].X)
                        break;
                j = j - 1;
                if (j == -1 || j == source.Length - 1)
                {
                    if (j == -1)
                        throw new Exception("插值下边界超出");
                    // ReSharper disable once CompareOfFloatsByEqualityOperator
                    if (j == source.Length - 1 && xs[i] == source[j].X)
                        insertRes[i] = source[j].Y;
                    else
                        throw new Exception("插值下边界超出");
                }
                else
                {
                    var p1 = (xs[i] - source[j + 1].X) / (source[j].X - source[j + 1].X);
                    p1 *= p1;
                    var p2 = (xs[i] - source[j].X) / (source[j + 1].X - source[j].X);
                    p2 *= p2;
                    var p3 =
                        p1
                        * (1 + 2 * (xs[i] - source[j].X) / (source[j + 1].X - source[j].X))
                        * source[j].Y
                        + p2
                        * (1 + 2 * (xs[i] - source[j + 1].X) / (source[j].X - source[j + 1].X))
                        * source[j + 1].Y;

                    var p4 =
                        p1 * (xs[i] - source[j].X) * m[j]
                        + p2 * (xs[i] - source[j + 1].X) * m[j + 1];
                    p4 += p3;
                    insertRes[i] = p4;
                }
            }

            return xs
                .Select(p => (int)p)
                .Zip(insertRes, (vx, vy) => (vx, vy))
                .Where(p => p.vx >= minX && p.vx <= maxX)
                .ToArray();
        }

        /// <summary>
        ///     Catmull-Rom插值(自动排序)
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static (int X, double Y)[] CatmullRomResample(this (int X, double Y)[] source)
        {
            source.SortX();

            if (source == null || source.Length < 2) throw new ArgumentException($"{nameof(source)} Length must >= 2");

            var c = source.Length;
            source = new[] { (2 * source[0].X - source[1].X, 2 * source[0].Y - source[1].Y) }
                .Concat(source)
                .Concat(new[] { (2 * source[c - 1].X - source[c - 2].X, 2 * source[c - 1].Y - source[c - 2].Y) })
                .ToArray();

            double CatmullRom(double p0, double p1, double p2, double p3, double t)
            {
                var t2 = t * t;
                var t3 = t2 * t;
                return 0.5 * (
                    2 * p1 +
                    (-p0 + p2) * t +
                    (2 * p0 - 5 * p1 + 4 * p2 - p3) * t2 +
                    (-p0 + 3 * p1 - 3 * p2 + p3) * t3
                );
            }

            var interpolatedPoints = new List<(int X, double Y)>();

            for (var i = 0; i < source.Length - 3; i++)
            {
                var p0 = source[i];
                var p1 = source[i + 1];
                var p2 = source[i + 2];
                var p3 = source[i + 3];

                //  p1 -> p2 插值
                for (var x = p1.X; x < p2.X; x++)
                {
                    var t = (double)(x - p1.X) / (p2.X - p1.X);

                    // 使用 Catmull-Rom 公式计算插值后的 Y 值
                    var y = CatmullRom(p0.Y, p1.Y, p2.Y, p3.Y, t);

                    interpolatedPoints.Add((x, y));
                }
            }

            // 添加最后一个点
            interpolatedPoints.Add(source[source.Length - 1 - 1]);

            return interpolatedPoints.ToArray();
        }
    }
}