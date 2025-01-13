using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Limxc.Tools.Extensions
{
    public static class RandomExtension
    {
        /// <summary>
        ///     由分布方法生成随机数组
        /// </summary>
        /// <param name="randomSource"></param>
        /// <param name="count"></param>
        /// <param name="distribution"></param>
        /// <returns></returns>
        public static double[] Generate(this Random randomSource, int count, Func<Random, double> distribution)
        {
            var rst = new double[count];
            for (var i = 0; i < count; i++) rst[i] = distribution(randomSource);

            Array.Sort(rst);

            return rst;
        }

        /// <summary>
        ///     概率密度, Probability Density
        /// </summary>
        /// <param name="source"></param>
        /// <param name="change"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static (int Number, int Count)[] ProbabilityDensity(this double[] source, Func<double, int> change,
            int min = int.MinValue, int max = int.MaxValue)
        {
            var dict = source
                .Select(change)
                .GroupBy(p => p)
                .ToDictionary(p => p.Key, p => p.Count());

            var rMin = dict.Select(p => p.Key).Min();
            if (min != int.MinValue)
                rMin = Math.Min(rMin, min);

            var rMax = dict.Select(p => p.Key).Max();
            if (max != int.MaxValue)
                rMax = Math.Max(rMax, max);

            var arr = Enumerable.Range(rMin, rMax - rMin + 1).Select(p => (p, 0)).ToArray();
            foreach (var k in dict) arr[k.Key - rMin] = (k.Key, k.Value);

            return arr;
        }

        /// <summary>
        ///     相同的key随机值一致, 并整体保证符合正态分布
        /// </summary>
        /// <param name="key">seed</param>
        /// <param name="center">中心值</param>
        /// <param name="scope">随机范围(正/负)</param>
        /// <param name="cutoff">在正态分布中截至上下限,也会改变分布密度</param>
        /// <param name="s">改变分布密度</param>
        /// <returns></returns>
        public static double NormalByKey(this string key, double center, double scope, int cutoff = 3, double s = 1.25)
        {
            int seed;
            // 使用稳定的哈希值作为种子
            using (var sha256 = SHA256.Create())
            {
                var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(key));
                seed = BitConverter.ToInt32(hashBytes, 0);
            }

            var random = new Random(seed);

            var r = random.Normal(0, s);

            //超范围使用均匀分布
            if (r < -cutoff || r > cutoff)
            {
                var n = Math.Abs(seed) % 1000; //0-1000
                return center - scope + (int)(2 * scope * n / 1000d);
            }

            return r * scope / cutoff + center;
        }

        #region From "https://github.com/conradshyu/rng"

        /// <summary>
        ///     均匀分布, default [0, 1]
        /// </summary>
        /// <param name="randomSource"></param>
        /// <param name="lower">下限</param>
        /// <param name="upper">上线</param>
        /// <returns></returns>
        public static double Uniform(this Random randomSource, double lower = 0.0, double upper = 1.0)
        {
            return lower + (upper - lower) * randomSource.NextDouble();
        }

        /// <summary>
        ///     高斯分布, 默认均值为0.0，标准差为1.0
        /// </summary>
        /// <param name="randomSource"></param>
        /// <param name="u">平均差</param>
        /// <param name="s">标准差</param>
        /// <returns></returns>
        public static double Gaussian(this Random randomSource, double u = 0.0, double s = 1.0)
        {
            return s * Math.Sqrt(-2.0 * Math.Log(randomSource.Uniform())) *
                Math.Cos(2.0 * Math.PI * randomSource.Uniform()) + u;
        }

        /// <summary>
        ///     正态分布, mean 0 and standard deviation 1
        /// </summary>
        /// <param name="randomSource"></param>
        /// <param name="u"></param>
        /// <param name="s"></param>
        /// <returns></returns>
        public static double Normal(this Random randomSource, double u = 0.0, double s = 1.0)
        {
            return randomSource.Gaussian(u, s);
        }

        /// <summary>
        ///     指数型分布, lambda 1
        /// </summary>
        /// <param name="randomSource"></param>
        /// <param name="l"></param>
        /// <returns></returns>
        public static double Exponential(this Random randomSource, double l = 1.0)
        {
            return -1.0 * Math.Log(randomSource.Uniform()) / l;
        }

        /// <summary>
        ///     伯努利分布, p of 0.5
        /// </summary>
        /// <param name="randomSource"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        public static bool Bernoulli(this Random randomSource, double p = 0.5)
        {
            return !(randomSource.Uniform() > p);
        }

        /// <summary>
        ///     贝塔分布, alpha 2 and beta 5, z = x / (x + y), z ~ beta(a, b), x ~ gamma(a, 1), y ~ gamma(b, 1)
        /// </summary>
        /// <param name="randomSource"></param>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static double Beta(this Random randomSource, double a = 2.0, double b = 5.0)
        {
            return 1.0 / (1.0 + randomSource.Gamma(b, 1.0) / randomSource.Gamma(a, 1.0));
        }

        /// <summary>
        ///     柯西分布, location 0 and scale 1
        /// </summary>
        /// <param name="randomSource"></param>
        /// <param name="x"></param>
        /// <param name="r"></param>
        /// <returns></returns>
        public static double Cauchy(this Random randomSource, double x = 0.0, double r = 1.0)
        {
            return r * Math.Tan(Math.PI * randomSource.Uniform()) + x;
        }


        /// <summary>
        ///     X平方分布, 默认自由度为10
        /// </summary>
        /// <param name="randomSource"></param>
        /// <param name="k"></param>
        /// <returns></returns>
        public static double Chi(this Random randomSource, int k = 10)
        {
            var c = 0.0;

            for (var i = 0; i < k; ++i) c += Math.Pow(randomSource.Normal(), 2.0);

            return c;
        }


        /// <summary>
        ///     埃尔朗分布, shape 2 and rate 0.5
        /// </summary>
        /// <param name="randomSource"></param>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static double Erlang(this Random randomSource, int a = 2, double b = 0.5)
        {
            var g = 0.0;

            for (var i = 0; i < a; ++i) g += randomSource.Exponential() / b;

            return g;
        }


        /// <summary>
        ///     F分布, 默认自由度为4和6
        /// </summary>
        /// <param name="randomSource"></param>
        /// <param name="d1"></param>
        /// <param name="d2"></param>
        /// <returns></returns>
        public static double F(this Random randomSource, int d1 = 4, int d2 = 6)
        {
            return randomSource.Chi(d1) * d2 / (randomSource.Chi(d2) * d1);
        }

        /// <summary>
        ///     伽马分布, shape 2.0 and rate 0.5
        /// </summary>
        /// <param name="randomSource"></param>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static double Gamma(this Random randomSource, double a = 2.0, double b = 0.5)
        {
            if (a < 1.0)
                return randomSource.Gamma(a + 1.0, b) * Math.Pow(randomSource.Uniform(), 1.0 / a);

            var d = a - 1.0 / 3.0;
            var c = 1.0 / Math.Sqrt(9.0 * d);
            double z, v, p;

            do
            {
                // iteratively find the random number
                z = randomSource.Normal();
                v = Math.Pow(1.0 + c * z, 3.0);
                p = 0.5 * Math.Pow(z, 2.0) + d - d * v + d * Math.Log(v);
            } while (z < -1.0 / c || Math.Log(randomSource.Uniform()) > p);

            return d * v / b;
        }


        /// <summary>
        ///     帕累托分布, scale 2 and shape 3
        /// </summary>
        /// <param name="randomSource"></param>
        /// <param name="x"></param>
        /// <param name="a"></param>
        /// <returns></returns>
        public static double Pareto(this Random randomSource, double x = 2.0, double a = 3.0)
        {
            return x * Math.Pow(randomSource.Uniform(), -1.0 / a);
        }

        /// <summary>
        ///     瑞利分布, sigma 0.5
        /// </summary>
        /// <param name="randomSource"></param>
        /// <param name="s"></param>
        /// <returns></returns>
        public static double Rayleigh(this Random randomSource, double s = 0.5)
        {
            return Math.Sqrt(-2.0 * Math.Pow(s, 2.0) * Math.Log(randomSource.Uniform()));
        }


        /// <summary>
        ///     韦伯分布, lambda 1 and k 1
        /// </summary>
        /// <param name="randomSource"></param>
        /// <param name="l"></param>
        /// <param name="k"></param>
        /// <returns></returns>
        public static double Weibull(this Random randomSource, double l = 1.0, double k = 1.0)
        {
            return l * Math.Pow(-1.0 * Math.Log(randomSource.Uniform()), 1.0 / k);
        }


        /// <summary>
        ///     T分布(学生分布), 默认自由度为10, T = Z / sqrt( X / n )
        /// </summary>
        /// <param name="randomSource"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public static double T(this Random randomSource, double n = 10.0)
        {
            return randomSource.Normal() / Math.Sqrt(randomSource.Chi((int)n) / n);
        }

        #endregion
    }
}