using FluentAssertions;
using Force.Crc32;
using System;
using System.Linq;
using Xunit;

namespace Limxc.Tools.DeviceComm.Extensions.Tests
{
    public class ChecksumExtensionTests
    {
        [Fact()]
        public void Crc32Test()
        {
            var bytes = "AA010203BB".ToByte();
            var r = bytes.Crc32();

            var bytes4 = new byte[bytes.Length + 4];
            Array.Copy(bytes, bytes4, bytes.Length);
            var r2uint = Crc32Algorithm.ComputeAndWriteToEnd(bytes4);
            var r2 = bytes4.Skip(bytes.Length).ToArray();

            r.Should().BeEquivalentTo(r2);

            //string 
            "AA0199BB".Crc32().Should().Be("06569143");
        }

        [Fact()]
        public void Crc32CTest()
        {
            var bytes = "AA010203BB".ToByte();
            var r = bytes.Crc32C();

            var bytes4 = new byte[bytes.Length + 4];
            Array.Copy(bytes, bytes4, bytes.Length);
            var r2uint = Crc32CAlgorithm.ComputeAndWriteToEnd(bytes4);
            var r2 = bytes4.Skip(bytes.Length).ToArray();

            r.Should().BeEquivalentTo(r2);
        }
    }
}