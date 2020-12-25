using FluentAssertions;
using System;
using System.Linq;
using Xunit;

namespace Limxc.Tools.DeviceComm.Extensions.Tests
{
    public class DataConversionExtensionTests
    {
        private int i1 = 211;
        private int i2 = int.MinValue;

        private string s1 = "Ab Cde fGH";
        private string s2 = "abcdef";

        [Fact()]
        public void ToStrArrayTest()
        {
            s1.ToStrArray(2).Should().BeEquivalentTo(new string[] { "Ab", "Cd", "ef", "GH" });

            s1.ToStrArray(4).Should().BeEquivalentTo(new string[] { "AbCd", "efGH" });
        }

        [Fact()]
        public void HexStrFormatTest()
        {
            "Ab Cde fG".HexStrFormat().Should().Be("0A BC DE FG");
            "Ab Cde fG".HexStrFormat(false, "").Should().Be("0ABCDEFG");
        }

        [Fact()]
        public void Int_HexStrTest()
        {
            i1.ToHexStr(2).ToNInt().Should().Be(i1);

            "A".ToNInt().Should().Be(10);

            26.ToHexStr(2).Should().Be("1a");
            26.ToHexStr(4).Should().Be("001a");

            i2.ToHexStr(8,true).ToNInt(true).Should().Be(i2.ToNInt(true));
        }

        [Fact()]
        public void Int_ByteTest()
        {
            new byte[] { 0, 1 }.ToNInt().Should().Be(1);
            new byte[] { 0, 0, 0, 1 }.ToNInt().Should().Be(1);
            new byte[] { 255, 255 }.ToNInt().Should().Be(65535);
            new byte[] { 0, 0, 255, 255 }.ToNInt().Should().Be(65535);//big endian
            new byte[] { 0, 1, 0, 0 }.ToNInt().Should().Be(65536);

            i1.ToBytes(4).ToNInt().Should().Be(i1);
            int.MaxValue.ToBytes(2).ToNInt(true).Should().Be(65535);
            int.MinValue.ToBytes(2, true).ToNInt(true).Should().Be(0);

            i1.ToBytes(2).ToNInt().Should().Be(i1);
            i2.ToBytes(4, true).ToNInt(true).Should().Be(0);
            65535.ToBytes(8).Should().BeEquivalentTo(new byte[] { 0, 0, 0, 0, 0, 0, 255, 255 });
        }

        [Fact()]
        public void ToIntArrayTest()
        {
            new string[] { "A", "1a" }.ToNInts().Should().BeEquivalentTo(new int[] { 10, 26 });
            "0A1a".ToNInts(2).Should().BeEquivalentTo(new int[] { 10, 26 });
        }

        [Fact()]
        public void Hex_ByteTest()
        {
            s2.ToByte().ToHexStr().ToLower().Should().Be(s2);

            "abcd".ToByte().Should().BeEquivalentTo(new byte[] { 171, 205 });
        }

        [Fact()]
        public void AscII_HexTest()
        {
            var data = "32 30 32 30 2C 31 32 2C 31 38 2C 31 30 2C 35 34 2C 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 2C 30 2C 31 34 38 2C 31 32 32 2C 20 38 31 2C 31 0D 0A "
                .Replace(" ", "");
            var asc = data.HexToAscII();
            asc.Should().Be("2020,12,18,10,54,                    ,0,148,122, 81,1\r\n");
            var hex = asc.AscIIToHex();
            hex.ToUpper().Should().Be(data);
        }

        [Fact()]
        public void HexStrMultiplyTest()
        {
            int i = 6;
            i.ToHexStr(2).HexStrMultiply(3).ToNInt().Should().Be(18);
        }

        [Fact()]
        public void FormatTest()
        {
            var rnd = new Random(Guid.NewGuid().GetHashCode());

            for (int i = 0; i < 100; i++)
            {
                var from = new byte[rnd.Next(1, 9) - 1].Concat(new byte[] { 55 }).ToArray();
                var toLen = rnd.Next(1, 9);
                var to = new byte[toLen - 1].Concat(new byte[] { 55 }).ToArray();
                from.ChangeLength(toLen).Should().BeEquivalentTo(to);
            }

            uint.MaxValue.ToNInt(true).Should().Be(int.MaxValue);
            uint.MinValue.ToNInt(true).Should().Be(0);

            int.MaxValue.ToNInt(true).Should().Be(int.MaxValue);
            int.MinValue.ToNInt(true).Should().Be(0);
        }
    }
}