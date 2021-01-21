using Limxc.Tools.DeviceComm.Protocol;
using System;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;

namespace Limxc.Tools.DeviceComm.Extensions
{
    public static class ProtocolExtension
    {
        public async static Task<byte[]> SendAsync(this IProtocol protocol, byte[] bytes, int waitMs)
        {
            var now = DateTimeOffset.Now;
            if (await protocol.SendAsync(bytes))
            {
                return await protocol.Received.SkipUntil(now).TakeUntil(now.AddMilliseconds(waitMs)).ToTask();
            }
            else
            {
                return new byte[0];
            }
        }
    }
}