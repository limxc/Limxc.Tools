using Limxc.Tools.DeviceComm.Entities;
using Limxc.Tools.DeviceComm.Extensions;
using Limxc.Tools.Extensions;
using SimpleTcp;
using System;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;

namespace Limxc.Tools.DeviceComm.Protocol
{
    /// <summary>
    /// 用作下位机服务器,连接客户端一般只有一台
    /// </summary>
    public class TcpClientProtocol_SST : IProtocol
    {
        private SimpleTcpClient _client;

        private ISubject<CPContext> _msg;

        private readonly string _serverIpPort;

        public TcpClientProtocol_SST(string serverIpPort)
        {
            if (!serverIpPort.CheckIpPort())
                throw new ArgumentException($"IpPort Error : {serverIpPort}");

            _serverIpPort = serverIpPort;

            _msg = new Subject<CPContext>();

            _client = new SimpleTcpClient(_serverIpPort);

            var connect = Observable
                            .FromEventPattern<ClientConnectedEventArgs>(h => _client.Events.Connected += h, h => _client.Events.Connected -= h)
                            .Select(_ => true)
                             ;

            var disconnect = Observable
                .FromEventPattern<ClientDisconnectedEventArgs>(h => _client.Events.Disconnected += h, h => _client.Events.Disconnected -= h)
                .Select(_ => false);

            ConnectionState = Observable
                .Merge(connect, disconnect)
                .Retry();

            Received = Observable
                            .FromEventPattern<DataReceivedEventArgs>(h => _client.Events.DataReceived += h, h => _client.Events.DataReceived -= h)
                            .Select(p => p.EventArgs.Data)
                            .Retry()
                            .Publish()
                            .RefCount()
                            //.Debug("receive")
                            ;

            History = _msg.AsObservable()
                            //.Debug("send")
                            .FindResponse(Received)
                            //.Debug("prase received")
                            ;
        }

        public IObservable<bool> ConnectionState { get; }
        public IObservable<byte[]> Received { get; }
        public IObservable<CPContext> History { get; }

        public void CleanUp()
        {
            _client?.Disconnect();
            _client?.Dispose();
            
            _msg?.OnCompleted();
            _msg = null;
        }

        public async Task<bool> SendAsync(CPContext context)
        {
            var cmdStr = context.Command.Build();

            await _client.SendAsync(cmdStr).ConfigureAwait(false);

            context.SendTime = DateTime.Now;
            _msg.OnNext(context);

            return await Task.FromResult(true);
        }

        public async Task<bool> SendAsync(byte[] bytes)
        {
            await _client.SendAsync(bytes).ConfigureAwait(false);

            return await Task.FromResult(true);
        }

        public Task<bool> OpenAsync()
        {
            _client.Connect();
            return Task.FromResult(true);
        }

        public Task<bool> CloseAsync()
        {
            _client.Disconnect();
            return Task.FromResult(true);
        }
    }
}