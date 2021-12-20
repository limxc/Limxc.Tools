using System;

namespace Limxc.Tools.Extensions
{
    public static class NumberExtension
    {
        /// <summary>
        /// 数值转换溢出时为int.MinValue
        /// </summary>
        /// <param name="value"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
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
        /// 数值转换溢出时抛出异常
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

        public static int TryInt(this object value, int defaultValue)
        {
            int.TryParse(value.ToString(), out var result);
            return result == 0 ? defaultValue : result;
        }


        public static double TryDouble(this object value, double defaultValue)
        {
            double.TryParse(value.ToString(), out var result);
            return result == 0 ? defaultValue : result;
        }

        public static float TryFloat(this object value, float defaultValue)
        {
            float.TryParse(value.ToString(), out var result);
            return result == 0 ? defaultValue : result;
        }

        public static decimal TryDecimal(this object value, decimal defaultValue)
        {
            decimal.TryParse(value.ToString(), out var result);
            return result == 0 ? defaultValue : result;
        }
    }
}