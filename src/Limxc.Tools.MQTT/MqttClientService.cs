using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Limxc.Tools.Extensions;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Extensions.External.RxMQTT.Client;
using MQTTnet.Extensions.ManagedClient;
using MQTTnet.Formatter;
using MQTTnet.Protocol;

namespace Limxc.Tools.MQTT
{
    public class MqttClientService : IDisposable
    {
        private readonly IRxMqttClient _client;
        private readonly CompositeDisposable _disposable = new CompositeDisposable();

        public MqttClientService()
        {
            var factory = new MqttFactory();
            _client = factory.CreateRxMqttClient();

            ConnectionState = _client.Connected;
        }

        public IObservable<bool> ConnectionState { get; }

        public void Dispose()
        {
            _disposable?.Dispose();
            _client?.Dispose();
        }

        #region Payload Builder

        private MqttApplicationMessage CreateMsg(string topic, string payload)
        {
            return new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(payload)
                .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.ExactlyOnce)
                .Build();
        }

        #endregion


        #region Start Stop

        /// <summary>
        ///     Start Client
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="setting"></param>
        /// <returns></returns>
        public Task StartAsync(string clientId, MqttSetting setting)
        {
            var options = new ManagedMqttClientOptionsBuilder()
                .WithAutoReconnectDelay(TimeSpan.FromSeconds(5))
                .WithClientOptions(
                    new MqttClientOptionsBuilder()
                        .WithClientId(clientId)
                        .WithTcpServer(opt =>
                        {
                            opt.Server = setting.MqttServerIp;
                            opt.Port = setting.MqttServerPort;
                        })
                        .WithCleanSession()
                        .WithProtocolVersion(MqttProtocolVersion.V500)
                        .WithTimeout(TimeSpan.FromSeconds(10))
                        .WithKeepAlivePeriod(TimeSpan.FromSeconds(5))
                        .WithCredentials(setting.UserName, setting.Password)
                        .Build()
                )
                .Build();

            return _client.StartAsync(options);
        }

        /// <summary>
        ///     Clear RPC Subscriptions & Stop Client
        /// </summary>
        /// <returns></returns>
        public Task StopAsync()
        {
            return _client.StopAsync();
        }

        #endregion

        #region Pub Sub

        /// <summary>
        ///     topic : xxx/yyy
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="payload"></param>
        /// <returns></returns>
        public Task PubAsync(string topic, string payload)
        {
            return _client.PublishAsync(CreateMsg(topic, payload));
        }

        /// <summary>
        ///     topic : xxx/yyy
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="payload"></param>
        /// <returns></returns>
        public Task PubAsync<T>(string topic, T payload)
        {
            return PubAsync(topic, payload.ToJson());
        }

        /// <summary>
        ///     topic : xxx/yyy
        /// </summary>
        /// <param name="topic"></param>
        /// <returns></returns>
        public IObservable<string> Sub(string topic)
        {
            return _client.Connect(topic).SelectPayload();
        }

        /// <summary>
        ///     topic : xxx/yyy
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="topic"></param>
        /// <returns></returns>
        public IObservable<T> Sub<T>(string topic)
        {
            return Sub(topic).Select(p => p.JsonTo<T>());
        }

        #endregion
    }
}
