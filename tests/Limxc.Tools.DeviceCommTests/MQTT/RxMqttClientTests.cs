//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading;
//using System.Threading.Tasks;
//using FluentAssertions;
//using Limxc.Tools.DeviceComm.MQTT;
//using Xunit;

//namespace Limxc.Tools.DeviceCommTests.MQTT
//{
//    public class RxMqttClientTests
//    {
//        [Fact]
//        public async Task RxMqttClientTest()
//        {
//            var rstList = new List<string>();

//            var server = "https://mqtt.respy.cn/";
//            var port = 1883;
//            var topic = @"/t/1";
//            var message = "testmsg";

//            var client1 = new RxMqttClient();
//            var client2 = new RxMqttClient();
//            await client1.Start("client1", server, port);
//            await client2.Start("client1", server, port);

//            var obs = client2.Sub(topic).Subscribe(r => rstList.Add(r));
//            await client1.Pub(topic, message, CancellationToken.None);

//            await Task.Delay(1000);

//            rstList.FirstOrDefault().Should().Be(message);
//        }
//    }
//}