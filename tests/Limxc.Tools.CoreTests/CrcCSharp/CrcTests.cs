using FluentAssertions;
using Limxc.Tools.Core.CrcCSharp;
using Limxc.Tools.Extensions.DevComm;
using System;
using System.Diagnostics;
using System.Text;
using Xunit;

namespace Limxc.Tools.CrcCSharp.Tests
{
    public class CrcTests
    {
        [Fact()]
        public void Test()
        {
            foreach (var p in CrcStdParams.StandartParameters.Values)
            {
                var crc = new Crc(p);
                if (!crc.IsRight())
                    Debugger.Break();
            }
        }

        [Fact()]
        public void Crc16Test()
        {
            var bytes = Encoding.ASCII.GetBytes("AA0199BB");
            var crc = new Crc(CrcStdParams.StandartParameters[CrcAlgorithms.Crc16Modbus]);

            var hash = crc.ComputeHash(bytes);

            var value = Convert.ToString(BitConverter.ToInt32(hash), 16);

            value.HexStrFormat().Should().Be("C7 C0");

            value.HexStrFormat(true).Should().Be("C0 C7");
        }

        public ulong ReverseBits(ulong ul, int valueLength)
        {
            ulong newValue = 0;

            for (int i = valueLength - 1; i >= 0; i--)
            {
                newValue |= (ul & 1) << i;
                ul >>= 1;
            }

            return newValue;
        }

        [Fact()]
        public void Crc32Test()
        {
            var bytes = Encoding.ASCII.GetBytes("AA0199BB");
            var crc = new Crc(CrcStdParams.StandartParameters[CrcAlgorithms.Crc32]);

            var hash = crc.ComputeHash(bytes);

            var value = Convert.ToString(BitConverter.ToInt32(hash), 16);

            value.HexStrFormat().Should().Be("06 56 91 43");
        }
    }
}