using System;
using System.Collections.Generic;
using System.Linq;
using Limxc.Tools.Integrations.CrcCSharp;

namespace Limxc.Tools.Extensions.Communication
{
    /// <summary>
    ///     简单校验算法 todo: 需要时继续补充
    /// </summary>
    public static class ValidationExtension
    {
        /// <summary>
        ///     累加和校验
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

        /// <summary>
        ///     尝试所有Crc算法
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static Dictionary<CrcAlgorithms, string> CrcAll(this byte[] bytes)
        {
            var dict = new Dictionary<CrcAlgorithms, string>();
            foreach (CrcAlgorithms ca in Enum.GetValues(typeof(CrcAlgorithms)))
            {
                if (ca == CrcAlgorithms.Undefined) continue;

                var crc = new Crc(CrcStdParams.StandartParameters[ca]);
                var rst = crc.ComputeHash(bytes);
                dict.Add(ca, rst.ByteToHex());
            }

            return dict;
        }

        /// <summary>
        ///     使用指定Crc算法校验
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="crcAlgorithms"></param>
        /// <returns></returns>
        public static byte[] Crc(this byte[] bytes, CrcAlgorithms crcAlgorithms)
        {
            var crc = new Crc(CrcStdParams.StandartParameters[crcAlgorithms]);
            var rst = crc.ComputeHash(bytes);
            return rst;
        }
    }
}