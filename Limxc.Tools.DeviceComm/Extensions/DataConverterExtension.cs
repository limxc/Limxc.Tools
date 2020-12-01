using System;
using System.Linq;

namespace Limxc.Tools.DeviceComm.Extensions
{
    public static class DataConverterExtension
    {
        #region int hexstr

        /// <summary>
        /// hexstr to int
        /// </summary>
        /// <param name="hexStr">
        /// </param>
        /// <returns>
        /// </returns>
        public static int ToInt(this string hexStr)
        {
            hexStr = hexStr.Replace(" ", "");

            return Convert.ToInt32(hexStr, 16);
        }

        /// <summary>
        /// int to hexstr
        /// </summary>
        /// <param name="value"></param>
        /// <param name="length">2(普通) or 4(高低)</param>
        /// <returns></returns>
        public static string ToHexStr(this int value, int length = 2)
        {
            string result = Convert.ToString(value, 16).Trim();//十进制数字转十六进制字符串

            result = result.PadLeft(length, '0');

            if (result.Length % 2 == 1)
                result = "0" + result;

            if (length < result.Length)//数值过大 导致长度增加
            {
                throw new Exception("数值过大导致长度增加");
            }

            return result;
        }

        /// <summary>
        /// hexstr[] to int[]
        /// </summary>
        /// <param name="hexStrs"></param>
        /// <returns></returns>
        public static int[] ToIntArray(this string[] hexStrs)
        {
            return hexStrs.Select(p => p.ToInt()).ToArray();
        }

        /// <summary>
        /// hexstr to hexstr[] to int[]
        /// </summary>
        /// <param name="hexStr"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static int[] ToIntArray(this string hexStr, int length)
        {
            return hexStr.ToStrArray(length).ToIntArray();
        }

        #endregion int hexstr

        #region Int Bytes

        /// <summary>
        /// int 转 byte数组 (Big Endian)
        /// </summary>
        /// <param name="value"></param>
        /// <param name="byteLength">2/4</param>
        /// <returns></returns>
        public static byte[] ToBytes(this int value)
        {
            var bytes = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);

            return bytes;
        }

        /// <summary>
        ///  byte数组 转 int (Big Endian)
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static int ToInt(this byte[] bytes)
        {
            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);
            return BitConverter.ToInt32(bytes, 0);
        }

        #endregion Int Bytes

        #region Byte HexStr

        /// <summary>
        /// byte数组转16进制字符串
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static string ToHexStr(this byte[] bytes)
        {
            string returnStr = "";
            if (bytes != null)
            {
                for (int i = 0; i < bytes.Length; i++)
                {
                    returnStr += bytes[i].ToString("X2");
                }
            }
            return returnStr;
        }
        public static string ToHexStrFromChar(this byte[] bytes)
        {
            string returnStr = "";
            if (bytes != null)
            {
                for (int i = 0; i < bytes.Length; i++)
                {
                    returnStr += (char)bytes[i];
                }
            }
            return returnStr;
        }
        /// <summary>
        /// 16进制字符串转byte数组
        /// </summary>
        /// <param name="hexString"></param>
        /// <returns></returns>
        public static byte[] ToByte(this string hexString)
        {
            hexString = hexString.Replace(" ", "");
            if ((hexString.Length % 2) != 0)
                throw new FormatException($"16进制字符串长度错误:{hexString.Length}");

            byte[] returnBytes = new byte[hexString.Length / 2];
            for (int i = 0; i < returnBytes.Length; i++)
                returnBytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
            return returnBytes;
        }

        #endregion Byte HexStr

        #region Calculate

        /// <summary>
        /// 16进制乘法
        /// </summary>
        /// <param name="value">16进制原数值</param>
        /// <param name="times">倍数</param>
        /// <returns></returns>
        public static string HexStrMultiply(this string value, int times)
        {
            var valueInt = value.ToInt();

            valueInt *= times;

            return valueInt.ToHexStr(2);
        }

        #endregion Calculate

        /// <summary>
        /// 处理数据区 切成数组(length) 普通 = 2 高低位 = 4
        /// </summary>
        /// <param name="hexStr">
        /// </param>
        /// <param name="length">
        /// </param>
        /// <returns>
        /// </returns>
        public static string[] ToStrArray(this string hexStr, int length)
        {
            hexStr = hexStr.Replace(" ", "");

            if (hexStr.Length < (length) || hexStr.Length % (length) != 0)
                throw new FormatException($"数据格式错误:{hexStr}");

            var ay = hexStr.ToCharArray();
            var rst = new string[ay.Length / (length)];
            for (int i = 0; i < ay.Length; i = i + length)
            {
                for (int j = 0; j < length; j++)
                {
                    rst[i / length] += ay[i + j].ToString();
                }
            }
            return rst;
        }

        /// <summary>
        /// 16进制字符串格式化, 添加空格
        /// </summary>
        /// <param name="hexStr"></param>
        /// <returns></returns>
        public static string HexStrFormat(this string hexStr)
        {
            if (string.IsNullOrWhiteSpace(hexStr))
                return string.Empty;

            hexStr = hexStr.ToUpper().Replace(" ", "");

            if (hexStr.Length % 2 == 1)
                hexStr = "0" + hexStr;

            for (int i = hexStr.Length - 2; i > 0; i = i - 2)
            {
                hexStr = hexStr.Insert(i, " ");
            }
            return hexStr;
        }
    }
}