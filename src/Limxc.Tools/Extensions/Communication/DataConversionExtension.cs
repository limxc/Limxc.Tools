using System;
using System.Linq;
using System.Text;

namespace Limxc.Tools.Extensions.Communication
{
    public static class DataConversionExtension
    {
        #region Calculate

        /// <summary>
        ///     hex to natural int * n to hex
        /// </summary>
        /// <param name="hex">16进制原数值</param>
        /// <param name="times">倍数</param>
        /// <param name="adjustRange"></param>
        /// <returns></returns>
        public static string HexMultiply(this string hex, int times, bool adjustRange = false)
        {
            var value = hex.HexToInt(adjustRange);

            value *= times;

            return value.IntToHex(hex.Length, adjustRange);
        }

        #endregion Calculate

        #region Format

        /// <summary>
        ///     length :
        ///     1 = (0~255)
        ///     2 = (0~65535) 1bit
        ///     3 = (0~16777215)
        ///     4 = (0~4294967295) 2bit
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static byte[] ChangeLength(this byte[] bytes, int length)
        {
            var len = bytes.Length;

            if (len > length)
                return bytes.Skip(bytes.Length - length).ToArray();

            if (len < length)
                return new byte[length - len]
                    .Concat(bytes)
                    .ToArray();

            return bytes;
        }

        /// <summary>
        ///     hex string format
        /// </summary>
        /// <param name="hex"></param>
        /// <param name="reverse"></param>
        /// <param name="separator"></param>
        /// <returns></returns>
        public static string HexFormat(
            this string hex,
            bool reverse = false,
            string separator = " "
        )
        {
            if (string.IsNullOrWhiteSpace(hex))
                return string.Empty;

            hex = hex.ToUpper().Replace(" ", "");

            if (hex.Length % 2 == 1)
                hex = "0" + hex;

            var arr = hex.ToStrArray(2);
            if (reverse)
                arr = hex.ToStrArray(2).Reverse().ToArray();

            return string.Join(separator, arr);
        }

        /// <summary>
        ///     natural int (0 ~ int.MaxValue)
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
        ///     natural int (0 ~ int.MaxValue)
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
        ///     string to string[]
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
            for (var i = 0; i < ay.Length; i += length)
            for (var j = 0; j < length; j++)
                rst[i / length] += ay[i + j].ToString();
            return rst;
        }

        #endregion Format

        #region Int / Hex string

        /// <summary>
        ///     hex to uint to natural int
        /// </summary>
        /// <param name="hex">
        /// </param>
        /// <param name="adjustRange"></param>
        /// <returns></returns>
        public static int HexToInt(this string hex, bool adjustRange = false)
        {
            return hex.HexToUInt().ToNInt(adjustRange);
        }

        /// <summary>
        ///     hex[] to uint[] to natural int[]
        /// </summary>
        /// <param name="hex"></param>
        /// <returns></returns>
        public static int[] HexToInt(this string[] hex)
        {
            return hex.Select(p => p.HexToInt()).ToArray();
        }

        /// <summary>
        ///     hex to hex[] to uint[] to natural int[]
        /// </summary>
        /// <param name="hex"></param>
        /// <param name="length">2/4/8</param>
        /// <returns></returns>
        public static int[] HexToInt(this string hex, int length)
        {
            return hex.ToStrArray(length).HexToInt().ToArray();
        }

        /// <summary>
        ///     hex to uint
        /// </summary>
        /// <param name="hex">
        /// </param>
        /// <returns></returns>
        public static uint HexToUInt(this string hex)
        {
            hex = hex.Replace(" ", "");

            return Convert.ToUInt32(hex, 16);
        }

        /// <summary>
        ///     int to natural int to ascii
        /// </summary>
        /// <param name="value"></param>
        /// <param name="length">2/4/8</param>
        /// <param name="adjustRange"></param>
        /// <returns></returns>
        public static string IntToHex(this int value, int length, bool adjustRange = false)
        {
            value = value.ToNInt(adjustRange);

            var result = Convert.ToString(value, 16).Trim();

            result = result.PadLeft(length, '0');

            if (result.Length % 2 == 1)
                result = "0" + result;

            if (length < result.Length)
                throw new Exception($"Result Length Changed. From {length} to {result.Length}");

            return result;
        }

        #endregion

        #region Int / Byte

        /// <summary>
        ///     byte[n] to byte[4] to natural int
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="adjustRange"></param>
        /// <returns></returns>
        public static int ByteToInt(this byte[] bytes, bool adjustRange = false)
        {
            bytes = bytes.ChangeLength(4);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);

            return BitConverter.ToUInt32(bytes, 0).ToNInt(adjustRange);
        }

        /// <summary>
        ///     int to natural int to byte[length]
        /// </summary>
        /// <param name="value"></param>
        /// <param name="length">2/4</param>
        /// <param name="adjustRange"></param>
        /// <returns></returns>
        public static byte[] IntToByte(this int value, int length, bool adjustRange = false)
        {
            var bytes = BitConverter.GetBytes(value.ToNInt(adjustRange));
            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);

            return bytes.ChangeLength(length);
        }

        #endregion

        #region Hex / Byte

        /// <summary>
        ///     byte[](char) to ascii
        /// </summary>
        /// <param name="charBytes"></param>
        /// <returns></returns>
        public static string ByteCharToHex(this byte[] charBytes)
        {
            var hex = string.Empty;
            if (charBytes != null)
                hex = charBytes.Aggregate(hex, (current, b) => current + (char)b);
            return hex;
        }

        /// <summary>
        ///     byte[] to ascii
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static string ByteToHex(this byte[] bytes)
        {
            var hex = "";
            if (bytes != null)
                hex = bytes.Aggregate(hex, (current, b) => current + b.ToString("X2"));
            return hex;
        }

        /// <summary>
        ///     hex to byte[]
        /// </summary>
        /// <param name="hex"></param>
        /// <returns></returns>
        public static byte[] HexToByte(this string hex)
        {
            hex = hex.Replace(" ", "");
            if (hex.Length % 2 != 0)
                throw new FormatException($"Hex String Length Error : {hex.Length}");

            var bytes = new byte[hex.Length / 2];

            for (var i = 0; i < bytes.Length; i++)
                bytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);

            return bytes;
        }

        #endregion

        #region AscII / Hex

        /// <summary>
        ///     ascii to hex
        /// </summary>
        /// <param name="ascii"></param>
        /// <returns></returns>
        // ReSharper disable once InconsistentNaming
        public static string AscIIToHex(this string ascii)
        {
            return Encoding.UTF8.GetBytes(ascii).ByteToHex();
        }

        /// <summary>
        ///     hex to ascii
        /// </summary>
        /// <param name="hex"></param>
        /// <returns></returns>
        // ReSharper disable once InconsistentNaming
        public static string HexToAscII(this string hex)
        {
            var chars = hex.HexToInt(2).Select(p => (char)p).ToArray();
            return new string(chars);
        }

        #endregion
    }
}