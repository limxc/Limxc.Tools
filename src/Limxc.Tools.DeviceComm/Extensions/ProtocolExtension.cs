using System;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using Limxc.Tools.DeviceComm.Protocol;

namespace Limxc.Tools.DeviceComm.Extensions
{
    public static class ProtocolExtension
    {
        public static async Task<byte[]> SendAsync(this IProtocol protocol, byte[] bytes, int waitMs)
        {
            var now = DateTimeOffset.Now;
            if (await protocol.SendAsync(bytes))
                return await protocol.Received.SkipUntil(now).TakeUntil(now.AddMilliseconds(waitMs)).ToTask();
            return new byte[0];
        }
    }
}