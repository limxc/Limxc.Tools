using System.IO;
using System.Text;
using System.Threading.Tasks;

// ReSharper disable InconsistentNaming

namespace Limxc.Tools.Extensions
{
    public static class EncodingDetectorExtension
    {
        /*
        https://github.com/dotnet-campus/EncodingNormalior

        UTF-8 | EF BB BF | 239 187 191
        UTF-16BE（大端序）| FE FF | 254 255
        UTF-16LE（小端序）| FF FE | 255 254
        UTF-32BE（大端序）| 00 00 FE FF | 0 0 254 255
        UTF-32LE（小端序）| FF FE 00 00 | 255 254 0 0
        UTF-7 | 2B 2F 76和以下的一个字节：[ 38 | 39 | 2B | 2F ] | 43 47 118和以下的一个字节：[ 56 | 57 | 43 | 47 ]
        en:UTF-1 | F7 64 4C | 247 100 76
        en:UTF-EBCDIC | DD 73 66 73 | 221 115 102 115
        en:Standard Compression Scheme for Unicode | 0E FE FF | 14 254 255
        en:BOCU-1 | FB EE 28及可能跟随着FF | 251 238 40及可能跟随着255
        GB-18030 | 84 31 95 33 | 132 49 149 51
        */

        /// <summary>239 187 191</summary>
        public static byte[] UTF8 = { 0xEF, 0xBB, 0xBF };

        /// <summary>255 254 65</summary>
        public static byte[] Unicode = { 0xFF, 0xFE, 0x41 };

        /// <summary>254 255 0</summary>
        public static byte[] UnicodeBIG = { 0xFE, 0xFF, 0x00 };

        /// <summary>43 47 118</summary>
        public static byte[] UTF7 = { 0x2B, 0x2F, 0x76 };

        /// <summary>0 0 254 255</summary>
        public static byte[] UTF32BIG = { 0X00, 0X00, 0xFE, 0xFF };

        /// <summary>255 254 0 0</summary>
        public static byte[] UTF32 = { 0xFF, 0xFE, 0x00, 0X00 };

        /// <summary>
        ///     UTF7 UTF8 Unicode BigEndian Unicode UTF32
        ///     ASCII UTF8 GBK
        /// </summary>
        public static async Task<Encoding> GetFileEncodingAsync(this string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException(filePath);

            //var fileInfo = new FileInfo(filePath);
            //if (fileInfo.Length < 5)
            //	return Encoding.Default;

            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                if (stream.Length < 2)
                    return Encoding.Default;

                // Analyze the BOM 
                var bom = new byte[4];
                _ = stream.Read(bom, 0, 4);
                stream.Position = 0;
                var encoding = AnalyzeBom(bom);

                //可能是GBK或无签名utf8, 根据字符数量判断
                if (encoding == null)
                {
                    var buffer = new byte[stream.Length];
                    _ = await stream.ReadAsync(buffer, 0, buffer.Length);

                    var countUtf8 = CountUTF8(buffer);
                    if (countUtf8 == 0)
                    {
                        encoding = Encoding.ASCII;
                    }
                    else
                    {
                        var countGbk = CountGBK(buffer);
                        if (countUtf8 > countGbk)
                            return Encoding.UTF8;
                        return Encoding.GetEncoding("GBK");
                    }
                }

                return encoding;
            }
        }

        /// <summary>
        ///     UTF7 UTF8 Unicode BigEndianUnicode UTF32
        ///     ASCII UTF8 GBK
        /// </summary>
        public static Encoding GetFileEncoding(this string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException(filePath);

            //var fileInfo = new FileInfo(filePath);
            //if (fileInfo.Length < 5)
            //	return Encoding.Default;

            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                if (stream.Length < 2)
                    return Encoding.Default;

                // Analyze the BOM 
                var bom = new byte[4];
                _ = stream.Read(bom, 0, 4);
                stream.Position = 0;
                var encoding = AnalyzeBom(bom);

                //可能是GBK或无签名utf8, 根据字符数量判断
                if (encoding == null)
                {
                    var buffer = new byte[stream.Length];
                    _ = stream.Read(buffer, 0, buffer.Length);

                    var countUtf8 = CountUTF8(buffer);
                    if (countUtf8 == 0)
                    {
                        encoding = Encoding.ASCII;
                    }
                    else
                    {
                        var countGbk = CountGBK(buffer);
                        if (countUtf8 > countGbk)
                            return Encoding.UTF8;
                        return Encoding.GetEncoding("GBK");
                    }
                }

                return encoding;
            }
        }

        private static Encoding AnalyzeBom(byte[] bom)
        {
            if (bom[0] == 0x2b && bom[1] == 0x2f && bom[2] == 0x76)
                return Encoding.UTF7;
            if (bom[0] == 0xef && bom[1] == 0xbb && bom[2] == 0xbf)
                return Encoding.UTF8;
            if (bom[0] == 0xff && bom[1] == 0xfe)
                return Encoding.Unicode;
            if (bom[0] == 0xfe && bom[1] == 0xff)
                return Encoding.BigEndianUnicode;
            if (bom[0] == 0 && bom[1] == 0 && bom[2] == 0xfe && bom[3] == 0xff)
                return Encoding.UTF32;
            return null;
        }

        private static int CountGBK(byte[] buffer)
        {
            var length = buffer.Length;
            var count = 0;

            for (var i = 0; i < length; i++)
            {
                if (buffer[i] <= 127)
                    continue;

                if (i + 1 >= length)
                    break;
                if (buffer[i] >= 161 && buffer[i] <= 247 &&
                    buffer[i + 1] >= 161 && buffer[i + 1] <= 254)
                {
                    count += 2;
                    i++;
                }
            }

            return count;
        }

        private static int CountUTF8(byte[] buffer)
        {
            var length = buffer.Length;
            var count = 0;

            const char head = (char)0x80;
            for (var i = 0; i < length; i++)
            {
                if (buffer[i] <= 127)
                    continue;

                var temp = buffer[i];
                var tempHead = head;
                var wordLength = 0;

                while ((temp & tempHead) != 0)
                {
                    wordLength++;
                    tempHead >>= 1;
                }

                if (wordLength <= 1)
                    continue;

                wordLength--;

                if (wordLength + i >= length)
                    break;
                var point = 1;
                for (; point <= wordLength; point++)
                    if (buffer[i + point] <= 127)
                        break;

                if (point > wordLength)
                {
                    count += wordLength + 1;
                    i += wordLength;
                }
            }

            return count;
        }
    }
}