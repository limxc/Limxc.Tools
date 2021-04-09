using System;
using System.Diagnostics;
using System.Linq;

namespace Limxc.Tools.Extensions
{
    public static class DataCompleterExtension
    {
        /// <summary>
        ///     线性补全
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static double[] MeanCompleter(this double?[] source)
        {
            var rst = new double?[source.Length];
            Array.Copy(source, rst, source.Length);

            var kv = source
                .Select((value, index) => (index, value))
                .Where(p => p.value.HasValue)
                .Select(p => new
                {
                    p.index,
                    value = (double) p.value
                })
                .ToArray();
            if (kv.Length > 2)
                for (var i = 1; i < kv.Length; i++)
                {
                    Func<int, double> CreateFunction()
                    {
                        var x1 = kv[i - 1].index;
                        var y1 = kv[i - 1].value;
                        var x2 = kv[i].index;
                        var y2 = kv[i].value;

                        double Fx(int x)
                        {
                            return x * ((y1 - y2) / (x1 - x2)) + (x1 * y2 - x2 * y1) / (x1 - x2);
                        }

                        return Fx;
                    }

                    //如果i之前有空值,生成函数,填充i之前的空值
                    if (rst.Take(kv[i].index).Any(p => p == null))
                    {
                        var fx = CreateFunction();

                        for (var j = 0; j < kv[i].index; j++)
                            if (rst[j] == null)
                                rst[j] = fx(j);
                    }

                    //如果i是最后一个且i之后有空值,填充i之后的空值 
                    if (i == kv.Length - 1 && rst.Skip(kv[i].index + 1).Any(p => p == null))
                    {
                        var fx = CreateFunction();

                        for (var j = kv[i].index + 1; j < rst.Length; j++)
                            if (rst[j] == null)
                                rst[j] = fx(j);
                    }
                }

            return rst.Select(p =>
            {
                Debug.Assert(p != null, nameof(p) + " != null");
                return (double) p;
            }).ToArray();
        }
    }
}