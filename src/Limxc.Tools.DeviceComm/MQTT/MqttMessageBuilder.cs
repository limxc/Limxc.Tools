using MQTTnet;

namespace Limxc.Tools.DeviceComm.MQTT
{
    public static class MqttMessageBuilder
    {
        public static MqttApplicationMessage CreateMsg(string topic, string payload)
        {
            return new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(payload)
                .WithAtMostOnceQoS()
                .WithRetainFlag()
                .Build();
        }
    }
}