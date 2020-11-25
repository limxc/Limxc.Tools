using FluentAssertions;
using Xunit;

namespace Limxc.Tools.DeviceComm.Extensions.Tests
{
    public class DataConverterExtensionTests
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
            s1.HexStrFormat().Should().Be("AB CD EF GH");
        }

        [Fact()]
        public void Int_HexStrTest()
        {
            i1.ToHexStr().ToInt().Should().Be(i1);

            
            "A".ToInt().Should().Be(10);

            26.ToHexStr(2).Should().Be("1a");
            26.ToHexStr(4).Should().Be("001a");

            i2.ToHexStr(8).ToInt().Should().Be(i2);
        }

        [Fact()]
        public void Int_ByteTest()
        {
            i1.ToBytes().ToInt().Should().Be(i1);
            i2.ToBytes().ToInt().Should().Be(i2);
        }

        [Fact()]
        public void ToIntArrayTest()
        {
            new string[] { "A", "1a" }.ToIntArray().Should().BeEquivalentTo(new int[] { 10, 26 });
            "0A1a".ToIntArray(2).Should().BeEquivalentTo(new int[] { 10, 26 });
        }

        [Fact()]
        public void Hex_ByteTest()
        {
            s2.ToByte().ToHexStr().ToLower().Should().Be(s2);

            "abcd".ToByte().Should().BeEquivalentTo(new byte[] { 171, 205 });
        }

        [Fact()]
        public void HexStrMultiplyTest()
        {
            int i = 6;
            //i.ToHexStr().HexStrMultiply();
        }
    }
}