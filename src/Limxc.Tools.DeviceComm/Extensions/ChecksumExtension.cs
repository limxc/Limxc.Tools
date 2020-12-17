using Limxc.Tools.DeviceComm.Utils;
using System.Text;

namespace Limxc.Tools.DeviceComm.Extensions
{
    public static class ChecksumExtension
    {
        public static byte[] Crc32(this byte[] source)
        {
            uint crc = 0;
            if (source.Length != 0)
            {
                crc = Crc32Helper.Crc32(source);
            }

            var rst = new byte[4];
            rst[0] = (byte)crc;
            rst[1] = (byte)(crc >> 8);
            rst[2] = (byte)(crc >> 16);
            rst[3] = (byte)(crc >> 24);
            return rst;
        }

        public static byte[] Crc32C(this byte[] source)
        {
            uint crc = 0;
            if (source.Length != 0)
            {
                crc = Crc32Helper.Crc32C(source);
            }

            var rst = new byte[4];
            rst[0] = (byte)crc;
            rst[1] = (byte)(crc >> 8);
            rst[2] = (byte)(crc >> 16);
            rst[3] = (byte)(crc >> 24);
            return rst;
        }

        public static string Crc32(this string source)
        {
            var bs = Encoding.ASCII.GetBytes(source);
            return Crc32Helper.Crc32(bs).ToString("X").HexStrFormat(false);
        }

        public static string Crc32C(this string source)
        {
            var bs = Encoding.ASCII.GetBytes(source);
            return Crc32Helper.Crc32C(bs).ToString("X").HexStrFormat(false);
        }
    }
}