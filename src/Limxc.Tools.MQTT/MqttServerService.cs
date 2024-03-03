using System;
using System.Diagnostics;
using System.Net;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using MQTTnet;
using MQTTnet.Diagnostics;
using MQTTnet.Protocol;
using MQTTnet.Server;

namespace Limxc.Tools.MQTT
{
    public class MqttServerService : IDisposable
    {
        private readonly Subject<(string, bool )> _connectionState = new Subject<(string, bool )>();
        private readonly MqttServer _server;
        private readonly string _serverClientId;

        public MqttServerService(MqttSetting setting = null, IMqttNetLogger logger = null)
        {
            _serverClientId = $"ServerAsClient_Id_{Guid.NewGuid()}";

            var builder = new MqttServerOptionsBuilder();
            builder = builder.WithDefaultEndpoint();

            if (!string.IsNullOrWhiteSpace(setting?.MqttServerIp) && setting?.MqttServerPort > 0)
                builder = builder
                    .WithDefaultEndpointBoundIPAddress(IPAddress.Parse(setting.MqttServerIp))
                    .WithDefaultEndpointPort(setting.MqttServerPort);

            var option = builder.WithKeepAlive().Build();

            _server = new MqttFactory(logger ?? new MqttNetDebugLogger()).CreateMqttServer(option);
            _server.ValidatingConnectionAsync += e =>
            {
                if (!string.IsNullOrWhiteSpace(setting?.UserName) && !string.IsNullOrWhiteSpace(setting.Password))
                    if (e.UserName != setting?.UserName || e.Password != setting?.Password)
                        e.ReasonCode = MqttConnectReasonCode.BadUserNameOrPassword;

                return Task.CompletedTask;
            };

            _server.ClientConnectedAsync
                += o =>
                {
                    _connectionState.OnNext((o.ClientId, true));
                    return Task.CompletedTask;
                };
            _server.ClientDisconnectedAsync
                += o =>
                {
                    _connectionState.OnNext((o.ClientId, false));
                    return Task.CompletedTask;
                };
        }

        public IObservable<(string ClientId, bool ConnState)> ConnectionState => _connectionState.AsObservable();


        public void Dispose()
        {
            _server?.Dispose();
            _connectionState.Dispose();
        }

        public Task StartAsync()
        {
            return _server.StartAsync();
        }

        public Task StopAsync()
        {
            return _server.StopAsync();
        }

        public Task PubAsync(string topic, string payload)
        {
            return _server.InjectApplicationMessage(CreateMessage(topic, payload));
        }

        private InjectedMqttApplicationMessage CreateMessage(string topic, string payload)
        {
            return new InjectedMqttApplicationMessage(new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(payload)
                .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.ExactlyOnce)
                .WithRetainFlag()
                .Build())
            {
                SenderClientId = _serverClientId
            };
        }
    }

    public class MqttNetDebugLogger : IMqttNetLogger
    {
        public void Publish(MqttNetLogLevel logLevel, string source, string message, object[] parameters,
            Exception exception)
        {
            if (parameters?.Length > 0) message = string.Format(message, parameters);
            switch (logLevel)
            {
                case MqttNetLogLevel.Verbose:
                    break;
                default:
                    Debug.WriteLine($"From MQTT Server({logLevel}) @ {DateTime.Now:hh:mm:ss}: {message}");
                    break;
            }
        }

        public bool IsEnabled => true;
    }
}