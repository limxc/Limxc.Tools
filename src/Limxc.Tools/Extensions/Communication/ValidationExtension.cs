using System.Linq;

namespace Limxc.Tools.Extensions.Communication
{
    /// <summary>
    ///     简单校验算法 todo: 遇见时继续补充 
    /// </summary>
    public static class ValidationExtension
    {
        /// <summary>
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static byte[] Checksum(this byte[] data)
        {
            var checksum = 0;
            foreach (var b in data) checksum += b;
            checksum = (checksum >> 16) + (checksum & 0xffff);
            checksum += checksum >> 16;
            checksum = ~checksum & 0xffff;

            return checksum.IntToByte(2).ToArray();
        }
    }
}