using System.Collections.Generic;
using Limxc.Tools.Integrations.CrcCSharp;

// ReSharper disable InconsistentNaming

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
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static byte Checksum(this byte[] bytes)
        {
            uint sum = 0;
            foreach (var b in bytes) sum += b;

            return (byte)(sum & 0xff);
        }

        /// <summary>
        ///     尝试所有Crc算法
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static IEnumerable<(CrcAlgorithms, byte[])> CrcAll(this byte[] bytes)
        {
            foreach (var parameter in CrcStdParams.StandartParameters)
            {
                if (parameter.Key == CrcAlgorithms.Undefined)
                    continue;

                var crc = new Crc(parameter.Value);
                var hash = crc.ComputeHash(bytes);
                yield return (parameter.Key, hash);
            }
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