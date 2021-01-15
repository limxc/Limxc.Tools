using System;
using System.Linq;
using System.Text;

namespace Limxc.Tools.Extensions.DevComm
{
    public static class DataConversionExtension
    {
        #region Format

        /// <summary>
        /// length :
        /// 1 = (0~255)
        /// 2 = (0~65535) 1bit
        /// 3 = (0~16777215)
        /// 4 = (0~4294967295) 2bit
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static byte[] ChangeLength(this byte[] bytes, int length)
        {
            var len = bytes.Length;

            if (len > length)
            {
                return bytes.Skip(bytes.Length - length).ToArray();
            }

            if (len < length)
            {
                return new byte[length - len].Concat(bytes).ToArray();
            }

            return bytes;
        }

        /// <summary>
        /// natural int (0 ~ int.MaxValue)
        /// </summary>
        /// <param name="value"></param>
        /// <param name="adjustRange"></param>
        /// <returns></returns>
        public static int ToNInt(this uint value, bool adjustRange)
        {
            if (!adjustRange && value > int.MaxValue)
                throw new ArgumentOutOfRangeException($"{value} is bigger than {int.MaxValue}");
            return value > int.MaxValue ? int.MaxValue : (int)value;
        }

        /// <summary>
        /// natural int (0 ~ int.MaxValue)
        /// </summary>
        /// <param name="value"></param>
        /// <param name="adjustRange"></param>
        /// <returns></returns>
        public static int ToNInt(this int value, bool adjustRange)
        {
            if (!adjustRange && value < 0)
                throw new ArgumentOutOfRangeException($"{value} is less than {0}");
            return value < 0 ? 0 : value;
        }

        /// <summary>
        /// hexstr format
        /// </summary>
        /// <param name="hexStr"></param>
        /// <param name="reverse"></param>
        /// <param name="separtor"></param>
        /// <returns></returns>
        public static string HexStrFormat(this string hexStr, bool reverse = false, string separtor = " ")
        {
            if (string.IsNullOrWhiteSpace(hexStr))
                return string.Empty;

            hexStr = hexStr.ToUpper().Replace(" ", "");

            if (hexStr.Length % 2 == 1)
                hexStr = "0" + hexStr;

            var arr = hexStr.ToStrArray(2);
            if (reverse)
                arr = hexStr.ToStrArray(2).Reverse().ToArray();

            return string.Join(separtor, arr);
        }

        /// <summary>
        /// string to string[]
        /// </summary>
        /// <param name="hexStr"></param>
        /// <param name="length">2/4/..</param>
        /// <returns> </returns>
        public static string[] ToStrArray(this string hexStr, int length)
        {
            hexStr = hexStr.Replace(" ", "");

            if (hexStr.Length < length || hexStr.Length % length != 0)
                throw new FormatException($"Length Error (2 or 4) : {hexStr}");

            var ay = hexStr.ToCharArray();
            var rst = new string[ay.Length / length];
            for (int i = 0; i < ay.Length; i += length)
            {
                for (int j = 0; j < length; j++)
                {
                    rst[i / length] += ay[i + j].ToString();
                }
            }
            return rst;
        }

        #endregion Format

        #region Int HexStr

        /// <summary>
        /// hexstr to uint
        /// </summary>
        /// <param name="hexStr">
        /// </param>
        /// <returns></returns>
        public static uint ToUInt(this string hexStr)
        {
            hexStr = hexStr.Replace(" ", "");

            return Convert.ToUInt32(hexStr, 16);
        }

        /// <summary>
        /// hexstr to natural int
        /// </summary>
        /// <param name="hexStr">
        /// </param>
        /// <returns></returns>
        public static int ToNInt(this string hexStr, bool adjustRange = false) => hexStr.ToUInt().ToNInt(adjustRange);

        /// <summary>
        /// natural int to hexstr
        /// </summary>
        /// <param name="value"></param>
        /// <param name="length">2/4/8</param>
        /// <param name="adjustRange"></param>
        /// <returns></returns>
        public static string ToHexStr(this int value, int length, bool adjustRange = false)
        {
            value = value.ToNInt(adjustRange);

            var result = Convert.ToString(value, 16).Trim();

            result = result.PadLeft(length, '0');

            if (result.Length % 2 == 1)
                result = "0" + result;

            if (length < result.Length)
            {
                throw new Exception($"Result Length Changed. From {length} to {result.Length}");
            }

            return result;
        }

        /// <summary>
        /// hexstr[] to natural int[]
        /// </summary>
        /// <param name="hexstrs"></param>
        /// <returns></returns>
        public static int[] ToNInts(this string[] hexstrs)
        {
            return hexstrs.Select(p => p.ToNInt()).ToArray();
        }

        /// <summary>
        /// hexstr to hexstr[] to int[]
        /// </summary>
        /// <param name="hexStr"></param>
        /// <param name="length">2/4/8</param>
        /// <returns></returns>
        public static int[] ToNInts(this string hexStr, int length)
        {
            return hexStr.ToStrArray(length).ToNInts().ToArray();
        }

        #endregion Int HexStr

        #region Int Bytes

        /// <summary>
        /// natural int to byte[length]
        /// </summary>
        /// <param name="value"></param>
        /// <param name="length">2/4</param>
        /// <returns></returns>
        public static byte[] ToBytes(this int value, int length, bool adjustRange = false)
        {
            var bytes = BitConverter.GetBytes(value.ToNInt(adjustRange));
            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);

            return bytes.ChangeLength(length);
        }

        /// <summary>
        /// byte[n] to byte[4] to natural int
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static int ToNInt(this byte[] bytes, bool adjustRange = false)
        {
            bytes = bytes.ChangeLength(4);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);

            return BitConverter.ToUInt32(bytes, 0).ToNInt(adjustRange);
        }

        #endregion Int Bytes

        #region HexStr Bytes

        /// <summary>
        /// byte[] to hexstr
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static string ToHexStr(this byte[] bytes)
        {
            var returnStr = "";
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
        /// byte[](char) to hexstr
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static string ToHexStrFromChar(this byte[] bytes)
        {
            var returnStr = string.Empty;
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
        /// hexstr to byte[]
        /// </summary>
        /// <param name="hexString"></param>
        /// <returns></returns>
        public static byte[] ToByte(this string hexString)
        {
            hexString = hexString.Replace(" ", "");
            if ((hexString.Length % 2) != 0)
                throw new FormatException($"Hex String Length Error : {hexString.Length}");

            var returnBytes = new byte[hexString.Length / 2];

            for (int i = 0; i < returnBytes.Length; i++)
                returnBytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);

            return returnBytes;
        }

        #endregion HexStr Bytes

        #region Calculate

        /// <summary>
        /// hexstr * n to hexstr
        /// </summary>
        /// <param name="value">16进制原数值</param>
        /// <param name="times">倍数</param>
        /// <param name="adjustRange"></param>
        /// <returns></returns>
        public static string HexStrMultiply(this string value, int times, bool adjustRange = false)
        {
            var valueInt = value.ToNInt(adjustRange);

            valueInt *= times;

            return valueInt.ToHexStr(value.Length, adjustRange);
        }

        #endregion Calculate

        #region AscII HexStr

        /// <summary>
        /// hex string to ascii string
        /// </summary>
        /// <param name="hexString"></param>
        /// <returns></returns>
        public static string HexToAscII(this string hexString)
        {
            var ca = hexString.ToNInts(2).Select(p => (char)p).ToArray();
            return new string(ca);
        }

        /// <summary>
        /// ascii string to hex string
        /// </summary>
        /// <param name="asciiString"></param>
        /// <returns></returns>
        public static string AscIIToHex(this string asciiString)
        {
            return Encoding.UTF8.GetBytes(asciiString).ToHexStr();
        }

        #endregion AscII HexStr
    }
}