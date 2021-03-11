using System;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Limxc.Tools.DeviceComm.Abstractions;
using MQTTnet;
using MQTTnet.Client.Options;
using MQTTnet.Extensions.External.RxMQTT.Client;
using MQTTnet.Extensions.ManagedClient;
using MQTTnet.Extensions.Rpc;
using MQTTnet.Extensions.Rpc.Options;
using MQTTnet.Formatter;
using MQTTnet.Protocol;
using Newtonsoft.Json;

namespace Limxc.Tools.DeviceComm.MQTT
{
    public class RxMqttClient : ICommClientService
    {
        private readonly IRxMqttClient _client;

        public RxMqttClient()
        {
            _client = new MqttFactory().CreateRxMqttClient();

            Connected = _client.Connected;
        }

        public IObservable<bool> Connected { get; }

        public Task Start(string clientId, string serverIp, int port)
        {
            var options = new ManagedMqttClientOptionsBuilder()
                .WithAutoReconnectDelay(TimeSpan.FromSeconds(5))
                .WithClientOptions(new MqttClientOptionsBuilder()
                    .WithClientId(clientId + Guid.NewGuid().ToString("D"))
                    .WithTcpServer(opt =>
                    {
                        opt.Server = serverIp;
                        opt.Port = port;
                    })
                    .WithCleanSession()
                    .WithProtocolVersion(MqttProtocolVersion.V500)
                    .WithCommunicationTimeout(TimeSpan.FromSeconds(10))
                    .WithKeepAlivePeriod(TimeSpan.FromSeconds(5))
                    //.WithCredentials(DeviceCommSettings.UserName, DeviceCommSettings.Password)
                    .Build())
                .Build();

            return _client.StartAsync(options);
        }

        public Task Stop()
        {
            return _client.StopAsync();
        }

        public void CleanUp()
        {
            _client.Dispose();
        }

        #region string方法

        /// <summary>
        ///     topic : /xxx/yyy
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="payload"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public Task Pub(string topic, string payload, CancellationToken token)
        {
            return _client.PublishAsync(MqttMessageBuilder.CreateMsg(topic.ToLower(), payload), token);
        }

        /// <summary>
        ///     topic : /xxx/yyy
        /// </summary>
        /// <typeparam name="TMsg"></typeparam>
        /// <param name="topic"></param>
        /// <returns></returns>
        public IObservable<string> Sub(string topic)
        {
            return _client
                .Connect(topic.ToLower())
                .SelectPayload();
        }

        /// <summary>
        ///     methodName : xxx.yyy
        /// </summary>
        /// <param name="methodName"></param>
        /// <param name="msg"></param>
        /// <param name="timeoutSeconds"></param>
        /// <returns></returns>
        public async Task<string> RpcPub(string methodName, string msg, int timeoutSeconds = 15)
        {
            var rpcOption = new MqttRpcClientOptions();
            //rpcOption.TopicGenerationStrategy.CreateRpcTopics(new MQTTnet.Extensions.Rpc.Options.TopicGeneration.TopicGenerationContext()
            //{
            //    MethodName = "",
            //    MqttClient = client.InternalClient,
            //    QualityOfServiceLevel = MqttQualityOfServiceLevel.ExactlyOnce
            //});

            var rpcClient = new MqttRpcClient(_client.InternalClient.InternalClient, rpcOption);

            var response = await rpcClient
                .ExecuteAsync(TimeSpan.FromSeconds(timeoutSeconds), methodName, msg,
                    MqttQualityOfServiceLevel.ExactlyOnce)
                .ConfigureAwait(false);

            return Encoding.UTF8.GetString(response);
        }

        /// <summary>
        ///     methodName : xxx.yyy
        /// </summary>
        /// <param name="methodName"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public IDisposable RpcSub(string methodName, Func<string, Task<string>> action)
        {
            return _client
                .Connect("MQTTnet.RPC/+/" + methodName)
                .Subscribe(async p =>
                {
                    var msg = string.Empty;
                    if (p.ApplicationMessage.Payload != null)
                        msg = Encoding.UTF8.GetString(p.ApplicationMessage.Payload);

                    var resp = await action(msg).ConfigureAwait(false);
                    await _client
                        .PublishAsync(MqttMessageBuilder.CreateMsg(p.ApplicationMessage.Topic + "/response", resp),
                            CancellationToken.None)
                        .ConfigureAwait(false);
                });
        }

        #endregion string方法

        #region 泛型方法

        /// <summary>
        ///     topic : /xxx/yyy
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="payload"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public Task Pub<T>(string topic, T payload, CancellationToken token)
        {
            return _client.PublishAsync(
                MqttMessageBuilder.CreateMsg(topic.ToLower(), JsonConvert.SerializeObject(payload)), token);
        }

        /// <summary>
        ///     topic : /xxx/yyy
        /// </summary>
        /// <typeparam name="TMsg"></typeparam>
        /// <param name="topic"></param>
        /// <returns></returns>
        public IObservable<T> Sub<T>(string topic)
        {
            return _client
                .Connect(topic.ToLower())
                .SelectPayload()
                .Select(p => JsonConvert.DeserializeObject<T>(p));
        }

        /// <summary>
        ///     methodName : xxx.yyy
        /// </summary>
        /// <param name="methodName"></param>
        /// <param name="msg"></param>
        /// <param name="timeoutSeconds"></param>
        /// <returns></returns>
        public async Task<TRst> RpcPub<TMsg, TRst>(string methodName, TMsg msg, int timeoutSeconds = 15)
        {
            var rpcOption = new MqttRpcClientOptions();
            //rpcOption.TopicGenerationStrategy.CreateRpcTopics(new MQTTnet.Extensions.Rpc.Options.TopicGeneration.TopicGenerationContext()
            //{
            //    MethodName = "",
            //    MqttClient = client.InternalClient,
            //    QualityOfServiceLevel = MqttQualityOfServiceLevel.ExactlyOnce
            //});

            var rpcClient = new MqttRpcClient(_client.InternalClient.InternalClient, rpcOption);

            var response = await rpcClient
                .ExecuteAsync(TimeSpan.FromSeconds(timeoutSeconds), methodName, JsonConvert.SerializeObject(msg),
                    MqttQualityOfServiceLevel.AtMostOnce)
                .ConfigureAwait(false);

            return JsonConvert.DeserializeObject<TRst>(Encoding.UTF8.GetString(response));
        }

        /// <summary>
        ///     methodName : xxx.yyy
        /// </summary>
        /// <param name="methodName"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public IDisposable RpcSub<TMsg, TRst>(string methodName, Func<TMsg, Task<TRst>> action)
        {
            return _client
                .Connect("MQTTnet.RPC/+/" + methodName)
                .Subscribe(async p =>
                {
                    var msg = string.Empty;
                    if (p.ApplicationMessage.Payload != null)
                        msg = Encoding.UTF8.GetString(p.ApplicationMessage.Payload);

                    var resp = await action(JsonConvert.DeserializeObject<TMsg>(msg)).ConfigureAwait(false);
                    await _client
                        .PublishAsync(
                            MqttMessageBuilder.CreateMsg(p.ApplicationMessage.Topic + "/response",
                                JsonConvert.SerializeObject(resp)), CancellationToken.None)
                        .ConfigureAwait(false);
                });
        }

        #endregion 泛型方法
    }
}