//using System;
//using System.Reactive.Disposables;
//using System.Reactive.Linq;
//using System.Text;
//using System.Text.Json;
//using System.Threading;
//using System.Threading.Tasks;
//using Limxc.Tools.Extensions;
//using MQTTnet;
//using MQTTnet.Client.Options;
//using MQTTnet.Extensions.External.RxMQTT.Client;
//using MQTTnet.Extensions.ManagedClient;
//using MQTTnet.Extensions.Rpc;
//using MQTTnet.Extensions.Rpc.Options;
//using MQTTnet.Formatter;
//using MQTTnet.Protocol;

//namespace Limxc.Tools.DeviceComm.MQTT
//{
//    public class RxMqttClient : IDisposable
//    {
//        private readonly IRxMqttClient _client;

//        private CompositeDisposable _disposable = new CompositeDisposable();

//        public RxMqttClient()
//        {
//            var factory = new MqttFactory();
//            _client = factory.CreateRxMqttClient();

//            ConnectionState = _client.Connected;
//        }

//        public IObservable<bool> ConnectionState { get; }

//        public void Dispose()
//        {
//            _disposable?.Dispose();
//            _client?.Dispose();
//        }

//        private string Serialize<T>(T obj)
//        {
//            return obj.ToJson();
//        }

//        private T Deserialize<T>(string obj)
//        {
//            return obj.JsonTo<T>();
//        }

//        #region Payload Builder

//        private MqttApplicationMessage CreateMsg(string topic, string payload)
//        {
//            return new MqttApplicationMessageBuilder()
//                .WithTopic(topic)
//                .WithPayload(payload)
//                .WithExactlyOnceQoS()
//                .Build();
//        }

//        #endregion


//        #region Start Stop

//        /// <summary>
//        ///     Start Client
//        /// </summary>
//        /// <param name="clientId"></param>
//        /// <param name="server"></param>
//        /// <param name="port"></param>
//        /// <returns></returns>
//        public Task Start(string clientId, string server, int port = 1883)
//        {
//            var options = new ManagedMqttClientOptionsBuilder()
//                .WithAutoReconnectDelay(TimeSpan.FromSeconds(5))
//                .WithClientOptions(new MqttClientOptionsBuilder()
//                    .WithClientId($"{(!string.IsNullOrWhiteSpace(clientId) ? $"{clientId}_" : "")}{Guid.NewGuid():D}")
//                    .WithTcpServer(opt =>
//                    {
//                        opt.Server = server;
//                        opt.Port = port;
//                    })
//                    .WithCleanSession()
//                    .WithProtocolVersion(MqttProtocolVersion.V500)
//                    .WithCommunicationTimeout(TimeSpan.FromSeconds(10))
//                    .WithKeepAlivePeriod(TimeSpan.FromSeconds(5))
//                    //.WithCredentials(DeviceCommSettings.UserName, DeviceCommSettings.Password)
//                    .Build())
//                .Build();

//            return _client.StartAsync(options);
//        }

//        /// <summary>
//        ///     Clear RPC Subscriptions & Stop Client
//        /// </summary>
//        /// <returns></returns>
//        public Task Stop()
//        {
//            _disposable.Dispose();
//            _disposable = new CompositeDisposable();
//            return _client.StopAsync();
//        }

//        #endregion

//        #region Pub Sub

//        /// <summary>
//        ///     topic : /xxx/yyy
//        /// </summary>
//        /// <param name="topic"></param>
//        /// <param name="payload"></param>
//        /// <param name="token"></param>
//        /// <returns></returns>
//        public Task Pub(string topic, string payload, CancellationToken token)
//        {
//            return _client.PublishAsync(CreateMsg(topic.ToLower(), payload), token);
//        }

//        /// <summary>
//        ///     topic : /xxx/yyy
//        /// </summary>
//        /// <param name="topic"></param>
//        /// <param name="payload"></param>
//        /// <param name="token"></param>
//        /// <returns></returns>
//        public Task Pub<T>(string topic, T payload, CancellationToken token)
//        {
//            return Pub(topic, Serialize(payload), token);
//        }


//        /// <summary>
//        ///     topic : /xxx/yyy
//        /// </summary>
//        /// <param name="topic"></param>
//        /// <returns></returns>
//        public IObservable<string> Sub(string topic)
//        {
//            return _client
//                .Connect(topic.ToLower())
//                .SelectPayload();
//        }

//        /// <summary>
//        ///     topic : /xxx/yyy
//        /// </summary>
//        /// <typeparam name="T"></typeparam>
//        /// <param name="topic"></param>
//        /// <returns></returns>
//        public IObservable<T> Sub<T>(string topic)
//        {
//            return Sub(topic).Select(Deserialize<T>);
//        }


//        /// <summary>
//        ///     methodName : xxx.yyy
//        /// </summary>
//        /// <param name="methodName"></param>
//        /// <param name="msg"></param>
//        /// <param name="timeoutSeconds"></param>
//        /// <returns></returns>
//        public async Task<string> RpcPub(string methodName, string msg, int timeoutSeconds = 15)
//        {
//            var rpcClient = new MqttRpcClient(_client.InternalClient.InternalClient, new MqttRpcClientOptions());

//            var response = await rpcClient
//                .ExecuteAsync(TimeSpan.FromSeconds(timeoutSeconds), methodName, msg,
//                    MqttQualityOfServiceLevel.ExactlyOnce)
//                .ConfigureAwait(false);

//            return Encoding.UTF8.GetString(response);
//        }

//        /// <summary>
//        ///     methodName : xxx.yyy
//        /// </summary>
//        /// <param name="methodName"></param>
//        /// <param name="msg"></param>
//        /// <param name="timeoutSeconds"></param>
//        /// <returns></returns>
//        public async Task<TRst> RpcPub<TMsg, TRst>(string methodName, TMsg msg, int timeoutSeconds = 15)
//        {
//            return Deserialize<TRst>(
//                await RpcPub(methodName, Serialize(msg), timeoutSeconds).ConfigureAwait(false));
//        }


//        /// <summary>
//        ///     methodName : xxx.yyy
//        /// </summary>
//        /// <param name="methodName"></param>
//        /// <param name="action"></param>
//        /// <returns></returns>
//        public void RpcSub(string methodName, Func<string, Task<string>> action)
//        {
//            var topic = "MQTTnet.RPC/+/" + methodName;

//            async void OnNext(MqttApplicationMessageReceivedEventArgs p)
//            {
//                var msg = string.Empty;
//                if (p.ApplicationMessage.Payload != null) msg = Encoding.UTF8.GetString(p.ApplicationMessage.Payload);

//                var resp = await action(msg).ConfigureAwait(false);
//                await _client.PublishAsync(CreateMsg(p.ApplicationMessage.Topic + "/response", resp),
//                        CancellationToken.None)
//                    .ConfigureAwait(false);
//            }

//            _client
//                .Connect(topic)
//                .Subscribe(OnNext).DisposeWith(_disposable);
//        }

//        /// <summary>
//        ///     methodName : xxx.yyy
//        /// </summary>
//        /// <param name="methodName"></param>
//        /// <param name="action"></param>
//        /// <returns></returns>
//        public void RpcSub<TMsg, TRst>(string methodName, Func<TMsg, Task<TRst>> action)
//        {
//            RpcSub(methodName, async t =>
//            {
//                try
//                {
//                    var msg = Deserialize<TMsg>(t);
//                    var res = await action(msg);
//                    return Serialize(res);
//                }
//                catch (Exception e)
//                {
//                    return e.Message;
//                }
//            });
//        }

//        #endregion
//    }
//}

