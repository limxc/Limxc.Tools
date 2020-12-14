using Limxc.Tools.Abstractions;
using MQTTnet;
using MQTTnet.Client.Options;
using MQTTnet.Extensions.External.RxMQTT.Client;
using MQTTnet.Extensions.ManagedClient;
using MQTTnet.Extensions.Rpc;
using MQTTnet.Extensions.Rpc.Options;
using MQTTnet.Formatter;
using MQTTnet.Protocol;
using Newtonsoft.Json;
using System;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Limxc.Tools.Core.MQTT
{
    public class RxMqttClient : ICommClientService
    {
        private IRxMqttClinet client;

        public RxMqttClient()
        {
            client = new MqttFactory().CreateRxMqttClient();

            Connected = client.Connected;
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
                  .WithCleanSession(true)
                  .WithProtocolVersion(MqttProtocolVersion.V500)
                  .WithCommunicationTimeout(TimeSpan.FromSeconds(10))
                  .WithKeepAlivePeriod(TimeSpan.FromSeconds(5))
                  //.WithCredentials(DeviceCommSettings.UserName, DeviceCommSettings.Password)
                  .Build())
              .Build();

            return client.StartAsync(options);
        }

        public Task Stop()
        {
            return client.StopAsync();
        }

        public void CleanUp()
        {
            client.Dispose();
        }

        #region string方法

        /// <summary>
        /// topic : /xxx/yyy
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="payload"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public Task Pub(string topic, string payload, CancellationToken token)
        {
            return client.PublishAsync(MqttMessageBuilder.CreateMsg(topic.ToLower(), payload), token);
        }

        /// <summary>
        /// topic : /xxx/yyy
        /// </summary>
        /// <typeparam name="TMsg"></typeparam>
        /// <param name="topic"></param>
        /// <returns></returns>
        public IObservable<string> Sub(string topic)
        {
            return client
                .Connect(topic.ToLower())
                .SelectPayload();
        }

        /// <summary>
        /// methodName : xxx.yyy
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

            var rpcClient = new MqttRpcClient(client.InternalClient.InternalClient, rpcOption);

            var response = await rpcClient.ExecuteAsync(TimeSpan.FromSeconds(timeoutSeconds), methodName, msg, MqttQualityOfServiceLevel.ExactlyOnce);

            return Encoding.UTF8.GetString(response);
        }

        /// <summary>
        /// methodName : xxx.yyy
        /// </summary>
        /// <param name="methodName"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public IDisposable RpcSub(string methodName, Func<string, Task<string>> action)
        {
            return client
                .Connect("MQTTnet.RPC/+/" + methodName)
                .Subscribe(async p =>
                {
                    string msg = string.Empty;
                    if (p.ApplicationMessage.Payload != null)
                        msg = Encoding.UTF8.GetString(p.ApplicationMessage.Payload);

                    var resp = await action(msg);
                    await client.PublishAsync(MqttMessageBuilder.CreateMsg(p.ApplicationMessage.Topic + "/response", resp), CancellationToken.None);
                });
        }

        #endregion string方法

        #region 泛型方法

        /// <summary>
        /// topic : /xxx/yyy
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="payload"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public Task Pub<T>(string topic, T payload, CancellationToken token)
        {
            return client.PublishAsync(MqttMessageBuilder.CreateMsg(topic.ToLower(), JsonConvert.SerializeObject(payload)), token);
        }

        /// <summary>
        /// topic : /xxx/yyy
        /// </summary>
        /// <typeparam name="TMsg"></typeparam>
        /// <param name="topic"></param>
        /// <returns></returns>
        public IObservable<T> Sub<T>(string topic)
        {
            return client
                .Connect(topic.ToLower())
                .SelectPayload()
                .Select(p => JsonConvert.DeserializeObject<T>(p));
        }

        /// <summary>
        /// methodName : xxx.yyy
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

            var rpcClient = new MqttRpcClient(client.InternalClient.InternalClient, rpcOption);

            var response = await rpcClient.ExecuteAsync(TimeSpan.FromSeconds(timeoutSeconds), methodName, JsonConvert.SerializeObject(msg), MqttQualityOfServiceLevel.AtMostOnce);

            return JsonConvert.DeserializeObject<TRst>(Encoding.UTF8.GetString(response));
        }

        /// <summary>
        /// methodName : xxx.yyy
        /// </summary>
        /// <param name="methodName"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public IDisposable RpcSub<TMsg, TRst>(string methodName, Func<TMsg, Task<TRst>> action)
        {
            return client
                .Connect("MQTTnet.RPC/+/" + methodName)
                .Subscribe(async p =>
                {
                    string msg = string.Empty;
                    if (p.ApplicationMessage.Payload != null)
                        msg = Encoding.UTF8.GetString(p.ApplicationMessage.Payload);

                    var resp = await action(JsonConvert.DeserializeObject<TMsg>(msg));
                    await client.PublishAsync(MqttMessageBuilder.CreateMsg(p.ApplicationMessage.Topic + "/response", JsonConvert.SerializeObject(resp)), CancellationToken.None);
                });
        }

        #endregion 泛型方法
    }
}