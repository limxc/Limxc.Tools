using System;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Limxc.Tools.Entities.Communication;
using Limxc.Tools.Extensions;
using SimpleTcp;

namespace Limxc.Tools.DeviceComm.Protocol
{
    /// <summary>
    ///     用作下位机服务器,连接客户端一般只有一台
    /// </summary>
    public class TcpClientProtocol : ProtocolBase
    {
        private SimpleTcpClient _client;
        public override bool IsConnected => _client?.IsConnected ?? false;

        public override void Init(params object[] pars)
        {
            var serverIpPort = (string) pars[0];
            Init(serverIpPort);
        }

        public void Init(string serverIpPort)
        {
            if (!serverIpPort.CheckIpPort())
                throw new ArgumentException($"IpPort Error : {serverIpPort}");

            _disposables?.Dispose();
            _disposables = new CompositeDisposable();

            _client?.Dispose();

            _client = new SimpleTcpClient(serverIpPort);

            var connect = Observable
                    .FromEventPattern<ClientConnectedEventArgs>(h => _client.Events.Connected += h,
                        h => _client.Events.Connected -= h)
                    .Select(_ => true)
                ;

            var disconnect = Observable
                .FromEventPattern<ClientDisconnectedEventArgs>(h => _client.Events.Disconnected += h,
                    h => _client.Events.Disconnected -= h)
                .Select(_ => false);

            connect
                .Merge(disconnect)
                .SubscribeOn(NewThreadScheduler.Default)
                .Subscribe(s => _connectionState.OnNext(s))
                .DisposeWith(_disposables);

            Observable
                .FromEventPattern<DataReceivedEventArgs>(h => _client.Events.DataReceived += h,
                    h => _client.Events.DataReceived -= h)
                .Select(p => p.EventArgs.Data)
                .SubscribeOn(NewThreadScheduler.Default)
                .Subscribe(b => _received.OnNext(b))
                .DisposeWith(_disposables);
        }

        public override async Task<bool> SendAsync(CommContext context)
        {
            var cmdStr = context.Command.Build();

            await _client.SendAsync(cmdStr).ConfigureAwait(false);

            context.SendTime = DateTime.Now;
            _history.OnNext(context);

            return await Task.FromResult(true).ConfigureAwait(false);
        }

        public override async Task<bool> SendAsync(byte[] bytes)
        {
            await _client.SendAsync(bytes).ConfigureAwait(false);
            await Task.Delay(50);
            return await Task.FromResult(true).ConfigureAwait(false);
        }

        public override Task<bool> OpenAsync()
        {
            _client.Connect();
            return Task.FromResult(true);
        }

        public override Task<bool> CloseAsync()
        {
            _client.Disconnect();
            return Task.FromResult(true);
        }

        protected override void Dispose(bool disposing)
        {
            _client?.Disconnect();
            _client?.Dispose();
            base.Dispose(disposing);
        }
    }
}