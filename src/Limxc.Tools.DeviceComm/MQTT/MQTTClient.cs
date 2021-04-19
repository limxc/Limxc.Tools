using System;
using System.Collections.Concurrent;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
    public class MQTTClient : IDisposable
    {
        private readonly IRxMqttClient _client;

        private readonly ConcurrentDictionary<string, IDisposable> _subscriptions =
            new ConcurrentDictionary<string, IDisposable>();

        public MQTTClient()
        {
            _client = new MqttFactory().CreateRxMqttClient();

            ConnectionState = _client.Connected;
        }

        public IObservable<bool> ConnectionState { get; }

        public void Dispose()
        {
            foreach (var dis in _subscriptions.Values) dis.Dispose();

            _client?.Dispose();
        }

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

        #region Payload Builder

        private MqttApplicationMessage CreateMsg(string topic, string payload)
        {
            return new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(payload)
                .WithExactlyOnceQoS()
                .Build();
        }

        #endregion


        #region Pub Sub

        /// <summary>
        ///     topic : /xxx/yyy
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="payload"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public Task Pub(string topic, string payload, CancellationToken token)
        {
            return _client.PublishAsync(CreateMsg(topic.ToLower(), payload), token);
        }

        /// <summary>
        ///     topic : /xxx/yyy
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="payload"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public Task Pub<T>(string topic, T payload, CancellationToken token)
        {
            return Pub(topic, JsonConvert.SerializeObject(payload), token);
        }


        /// <summary>
        ///     topic : /xxx/yyy
        /// </summary>
        /// <param name="topic"></param>
        /// <returns></returns>
        public IObservable<string> Sub(string topic)
        {
            return _client
                .Connect(topic.ToLower())
                .SelectPayload();
        }

        /// <summary>
        ///     topic : /xxx/yyy
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="topic"></param>
        /// <returns></returns>
        public IObservable<T> Sub<T>(string topic)
        {
            return Sub(topic).Select(JsonConvert.DeserializeObject<T>);
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
            var rpcClient = new MqttRpcClient(_client.InternalClient.InternalClient, new MqttRpcClientOptions());

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
        /// <param name="msg"></param>
        /// <param name="timeoutSeconds"></param>
        /// <returns></returns>
        public async Task<TRst> RpcPub<TMsg, TRst>(string methodName, TMsg msg, int timeoutSeconds = 15)
        {
            return JsonConvert.DeserializeObject<TRst>(
                await RpcPub(methodName, JsonConvert.SerializeObject(msg), timeoutSeconds).ConfigureAwait(false));
        }


        /// <summary>
        ///     methodName : xxx.yyy
        /// </summary>
        /// <param name="methodName"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public void RpcSub(string methodName, Func<string, Task<string>> action)
        {
            var topic = "MQTTnet.RPC/+/" + methodName;
            if (_subscriptions.ContainsKey(topic))
                _subscriptions[topic].Dispose();

            var dis = _client
                .Connect(topic)
                .Subscribe(async p =>
                {
                    var msg = string.Empty;
                    if (p.ApplicationMessage.Payload != null)
                        msg = Encoding.UTF8.GetString(p.ApplicationMessage.Payload);

                    var resp = await action(msg).ConfigureAwait(false);
                    await _client
                        .PublishAsync(CreateMsg(p.ApplicationMessage.Topic + "/response", resp),
                            CancellationToken.None)
                        .ConfigureAwait(false);
                });
            _subscriptions.TryAdd(topic, dis);
        }

        /// <summary>
        ///     methodName : xxx.yyy
        /// </summary>
        /// <param name="methodName"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public void RpcSub<TMsg, TRst>(string methodName, Func<TMsg, Task<TRst>> action)
        {
            RpcSub(methodName, async t =>
            {
                try
                {
                    var msg = JsonConvert.DeserializeObject<TMsg>(t);
                    var res = await action(msg);
                    return JsonConvert.SerializeObject(res);
                }
                catch (Exception e)
                {
                    return e.Message;
                }
            });
        }

        #endregion
    }
}