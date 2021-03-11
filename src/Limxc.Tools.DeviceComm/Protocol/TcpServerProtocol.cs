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
    public class TcpServerProtocol : ProtocolBase
    {
        private string _clientIpPort = string.Empty;
        private SimpleTcpServer _server;
        public override bool IsConnected => _server?.IsListening ?? false;

        public void Init(string ipPort, string clientIpPort)
        {
            _clientIpPort = clientIpPort;

            if (!ipPort.CheckIpPort())
                throw new ArgumentException($"IpPort Error : {ipPort}");

            _disposables?.Dispose();
            _disposables = new CompositeDisposable();

            _server?.Dispose();

            _server = new SimpleTcpServer(ipPort);

            var connect = Observable
                .FromEventPattern<ClientConnectedEventArgs>(h => _server.Events.ClientConnected += h,
                    h => _server.Events.ClientConnected -= h)
                .Select(p => (p.EventArgs.IpPort, true));

            var disconnect = Observable
                .FromEventPattern<ClientDisconnectedEventArgs>(h => _server.Events.ClientDisconnected += h,
                    h => _server.Events.ClientDisconnected -= h)
                .Select(p => (p.EventArgs.IpPort, false));


            connect
                .Merge(disconnect)
                .Select(p => p.Item2)
                .StartWith(false)
                .DistinctUntilChanged()
                .SubscribeOn(NewThreadScheduler.Default)
                .Subscribe(s => _connectionState.OnNext(s))
                .DisposeWith(_disposables);


            Observable
                .FromEventPattern<DataReceivedEventArgs>(h => _server.Events.DataReceived += h,
                    h => _server.Events.DataReceived -= h)
                .Select(p => p.EventArgs.Data)
                .SubscribeOn(NewThreadScheduler.Default)
                .Subscribe(b => _received.OnNext(b))
                .DisposeWith(_disposables);
        }

        public override async Task<bool> SendAsync(CommContext context)
        {
            if (_clientIpPort.CheckIpPort())
            {
                var cmdStr = context.Command.Build();

                await _server.SendAsync(_clientIpPort, cmdStr).ConfigureAwait(false);

                context.SendTime = DateTime.Now;
                _history.OnNext(context);

                return await Task.FromResult(true).ConfigureAwait(false);
            }

            return await Task.FromResult(false).ConfigureAwait(false);
        }

        public override async Task<bool> SendAsync(byte[] bytes)
        {
            if (_clientIpPort.CheckIpPort())
            {
                await _server.SendAsync(_clientIpPort, bytes).ConfigureAwait(false);
                return await Task.FromResult(true).ConfigureAwait(false);
            }

            return await Task.FromResult(false).ConfigureAwait(false);
        }

        public override async Task<bool> OpenAsync()
        {
            await _server.StartAsync().ConfigureAwait(false);
            return await Task.FromResult(true).ConfigureAwait(false);
        }

        public override Task<bool> CloseAsync()
        {
            _server.Stop();
            return Task.FromResult(true);
        }

        protected override void Dispose(bool disposing)
        {
            _server?.Stop();
            _server?.Dispose();
            base.Dispose(disposing);
        }
    }
}