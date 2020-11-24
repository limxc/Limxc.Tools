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
        /// <param name="values"></param>
        /// <param name="length">1(普通) or 2(高低)</param>
        /// <param name="canLengthChange"></param>
        /// <returns></returns>
        public static string ToHexStr(this int values, int length = 2, bool canLengthChange = false)
        {
            string result = Convert.ToString(values, 16).Trim();//十进制数字转十六进制字符串

            result = result.PadLeft(length, '0');

            if (result.Length % 2 == 1)
                result = "0" + result;

            if (length < result.Length && !canLengthChange)//数值过大 导致长度增加
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

        #endregion



        #region Int Bytes

        /// <summary>
        /// int 转 byte数组(2/4)
        /// </summary>
        /// <param name="i"></param>
        /// <param name="byteLength">2/4</param>
        /// <returns></returns>
        public static byte[] ToBytes(this int i, int byteLength = 2)
        {
            if (byteLength != 2 && byteLength != 4)
                throw new NotSupportedException($"{nameof(byteLength)} : 2或4");

            byte[] result = new byte[byteLength];
            if (byteLength == 2)
            {
                result[0] = (byte)((i >> 8) & 0xFF);
                result[1] = (byte)(i & 0xFF);
            }
            if (byteLength == 4)
            {
                result[0] = (byte)((i >> 24) & 0xFF);
                result[1] = (byte)((i >> 16) & 0xFF);
                result[2] = (byte)((i >> 8) & 0xFF);
                result[3] = (byte)(i & 0xFF);
            }
            return result;
        }

        /// <summary>
        ///  byte数组(2/4) 转 int
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static int ToInt(this byte[] bytes)
        {
            var byteLength = bytes.Length;
            if (byteLength != 2 && byteLength != 4)
                throw new NotSupportedException($"数组长度 : 2或4");

            int num = -1;

            if (byteLength == 2)
            {
                num = bytes[1] & 0xFF;
                num |= ((bytes[0] << 8) & 0xFF00);
            }
            if (byteLength == 4)
            {
                num = bytes[3] & 0xFF;
                num |= ((bytes[2] << 8) & 0xFF00);
                num |= ((bytes[1] << 16) & 0xFF0000);
                num |= ((bytes[0] << 24) & 0xFF0000);
            }

            return num;
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
        /// <returns>返回16进制带分割的字符串 如"01 E0"</returns>
        public static string HexStrMultiply(string value, int times, bool canLengthChange = false)
        {
            var valueInt = value.ToInt();

            valueInt *= times;

            return valueInt.ToHexStr(1, canLengthChange);
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