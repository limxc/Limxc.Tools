using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Limxc.Tools.Extensions;
using Limxc.Tools.MQTT;
using Xunit;

namespace Limxc.Tools.MQTTTests;

public class MqttServiceTests
{
    private const string TOPIC = "TestTopic";
    private const string CLIENTID = "ClientID";
    private const string MSGFromServer = "ServerMsg";
    private const string MSGFromClient = "ClientMsg";

    private async Task MqttTest(MqttSetting setting)
    {
        var serverConnectionState = false;
        var clientConnectionState = false;
        var recvs = new List<string>();
        var disposeables = new CompositeDisposable();

        var server = new MqttServerService(setting);
        server
            .ConnectionState.Subscribe(p => serverConnectionState = p.ConnState)
            .DisposeWith(disposeables);
        await server.StartAsync();

        var client = new MqttClientService();
        client.ConnectionState.Subscribe(p => clientConnectionState = p).DisposeWith(disposeables);
        client.Sub(TOPIC).Subscribe(p => recvs.Add(p)).DisposeWith(disposeables);
        await client.StartAsync(CLIENTID, setting);

        server
            .ConnectionState.Select(p => p.ConnState)
            .CallAsync(async _ => { await server.PubAsync(TOPIC, MSGFromServer); })
            .Subscribe()
            .DisposeWith(disposeables);
        client
            .ConnectionState.CallAsync(async _ => await client.PubAsync(TOPIC, MSGFromClient))
            .Subscribe()
            .DisposeWith(disposeables);

        await Task.Delay(1000);

        serverConnectionState.Should().BeTrue();
        clientConnectionState.Should().BeTrue();
        recvs.Should().Contain(MSGFromServer);
        recvs.Should().Contain(MSGFromClient);

        disposeables.Dispose();
        server.Dispose();
        client.Dispose();
    }

    [Fact]
    public Task MqttServiceDefaultTest()
    {
        return MqttTest(
            new MqttSetting
            {
                MqttServerIp = "127.0.0.1",
                MqttServerPort = 1884,
                UserName = "UNAME",
                Password = "PWD"
            }
        );
    }

    [Fact]
    public Task MqttServiceWithSettingTest()
    {
        return MqttTest(new MqttSetting { MqttServerIp = "127.0.0.1", MqttServerPort = 1883 });
    }
}