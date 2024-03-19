namespace Limxc.Tools.MQTT
{
    public class MqttSetting
    {
        public string MqttServerIp { get; set; } = "127.0.0.1";
        public int MqttServerPort { get; set; } = 1883;
        public string UserName { get; set; }
        public string Password { get; set; }
    }
}