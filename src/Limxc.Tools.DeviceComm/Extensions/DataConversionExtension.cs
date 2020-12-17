using System;
using System.Linq;
using System.Text;

namespace Limxc.Tools.DeviceComm.Extensions
{
    public static class DataConversionExtension
    {
        #region Int HexStr

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
        /// <param name="length">2/4/8</param>
        /// <returns></returns>
        public static string ToHexStr(this int value, int length)
        {
            string result = Convert.ToString(value, 16).Trim();//十进制数字转十六进制字符串

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
        /// <param name="length">2/4/8</param>
        /// <returns></returns>
        public static int[] ToIntArray(this string hexStr, int length)
        {
            return hexStr.ToStrArray(length).ToIntArray().ToArray();
        }

        #endregion Int HexStr

        #region Int Bytes

        /// <summary>
        /// int to byte[2]/byte[4]
        /// </summary>
        /// <param name="value"></param>
        /// <param name="length">2/4</param>
        /// <returns></returns>
        public static byte[] ToBytes(this int value, int length)
        {
            if (length == 2)
            {
                byte[] result = new byte[2];
                result[0] = (byte)((value >> 8) & 0xFF);
                result[1] = (byte)(value & 0xFF);
                return result;
            }

            var bytes = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);

            return bytes;

            //if (byteLength != 2 && byteLength != 4)
            //    throw new NotSupportedException($"Bytes Length Should Be : 2 or 4");

            //byte[] result = new byte[byteLength];
            //if (byteLength == 2)
            //{
            //    result[0] = (byte)((value >> 8) & 0xFF);
            //    result[1] = (byte)(value & 0xFF);
            //}
            //if (byteLength == 4)
            //{
            //    result[0] = (byte)((value >> 24) & 0xFF);
            //    result[1] = (byte)((value >> 16) & 0xFF);
            //    result[2] = (byte)((value >> 8) & 0xFF);
            //    result[3] = (byte)(value & 0xFF);
            //}
            //return result;
        }

        /// <summary>
        ///  byte[2]/byte[4] to int
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static int ToInt(this byte[] bytes)
        {
            if (bytes.Length == 2)
            {
                int num = bytes[1] & 0xFF;
                num |= ((bytes[0] << 8) & 0xFF00);
                return num;
            }

            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);
            return BitConverter.ToInt32(bytes, 0);

            //var byteLength = bytes.Length;
            //if (byteLength != 2 && byteLength != 4)
            //    throw new NotSupportedException($"Bytes Length Should Be : 2 or 4");

            //int num = -1;

            //if (byteLength == 2)
            //{
            //    num = bytes[1] & 0xFF;
            //    num |= ((bytes[0] << 8) & 0xFF00);
            //}
            //if (byteLength == 4)
            //{
            //    num = bytes[3] & 0xFF;
            //    num |= ((bytes[2] << 8) & 0xFF00);
            //    num |= ((bytes[1] << 16) & 0xFF0000);
            //    num |= ((bytes[0] << 24) & 0xFF0000);
            //}

            //return num;
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
        /// byte[](char) to hexstr
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
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
        /// hexstr to byte[]
        /// </summary>
        /// <param name="hexString"></param>
        /// <returns></returns>
        public static byte[] ToByte(this string hexString)
        {
            hexString = hexString.Replace(" ", "");
            if ((hexString.Length % 2) != 0)
                throw new FormatException($"Hex String Length Error : {hexString.Length}");

            byte[] returnBytes = new byte[hexString.Length / 2];
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
        /// <returns></returns>
        public static string HexStrMultiply(this string value, int times)
        {
            var valueInt = value.ToInt();

            valueInt *= times;

            return valueInt.ToHexStr(2);
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
            var ca = hexString.ToIntArray(2).Select(p => (char)p).ToArray();
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

        /// <summary>
        /// string to string[]
        /// </summary>
        /// <param name="hexStr"></param>
        /// <param name="length">2/4/..</param>
        /// <returns> </returns>
        public static string[] ToStrArray(this string hexStr, int length)
        {
            hexStr = hexStr.Replace(" ", "");

            if (hexStr.Length < (length) || hexStr.Length % (length) != 0)
                throw new FormatException($"Length Error (2 or 4) : {hexStr}");

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
        /// hexstr format
        /// </summary>
        /// <param name="hexStr"></param>
        /// <param name="spaceSplit"></param>
        /// <returns></returns>
        public static string HexStrFormat(this string hexStr, bool spaceSplit = true)
        {
            if (string.IsNullOrWhiteSpace(hexStr))
                return string.Empty;

            hexStr = hexStr.ToUpper().Replace(" ", "");

            if (hexStr.Length % 2 == 1)
                hexStr = "0" + hexStr;

            if (spaceSplit)
                for (int i = hexStr.Length - 2; i > 0; i = i - 2)
                {
                    hexStr = hexStr.Insert(i, " ");
                }
            return hexStr;
        }
    }
}