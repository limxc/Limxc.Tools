namespace Limxc.Tools.MQTT
{
    public class MqttSetting
    {
        public virtual string MqttServerIp { get; set; } = "127.0.0.1";
        public virtual int MqttServerPort { get; set; } = 1883;
        public virtual string UserName { get; set; }
        public virtual string Password { get; set; }
    }
}