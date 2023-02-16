using System;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Limxc.Tools.Bases.Communication;
using Limxc.Tools.Extensions;
using SuperSimpleTcp;

namespace Limxc.Tools.DeviceComm.Protocol
{
    /// <summary>
    ///     用作下位机服务器,连接客户端一般只有一台
    /// </summary>
    public class TcpServerProtocol : ProtocolBase
    {
        private string _clientIp = string.Empty;
        private SimpleTcpServer _server;
        public override bool IsConnected => _server?.IsListening ?? false;

        public override void Init(params object[] pars)
        {
            var ipPort = (string)pars[0];
            var clientIpPort = (string)pars[1];
            Init(ipPort, clientIpPort);
        }

        public void Init(string serverIpPort, string clientIp)
        {
            _clientIp = clientIp.Trim();

            if (!clientIp.CheckIp())
                throw new ArgumentException($"ClientIp Error: {clientIp}");

            serverIpPort = serverIpPort.Trim();
            if (!serverIpPort.CheckIpPort())
                throw new ArgumentException($"ServerIpPort Error : {serverIpPort}");

            _disposables?.Dispose();
            _disposables = new CompositeDisposable();

            _server?.Dispose();

            _server = new SimpleTcpServer(serverIpPort);

            var connect = Observable
                .FromEventPattern<ConnectionEventArgs>(h => _server.Events.ClientConnected += h,
                    h => _server.Events.ClientConnected -= h)
                .Where(p => p.EventArgs.IpPort.StartsWith(clientIp))
                .Select(p => true);

            var disconnect = Observable
                .FromEventPattern<ConnectionEventArgs>(h => _server.Events.ClientDisconnected += h,
                    h => _server.Events.ClientDisconnected -= h)
                .Where(p => p.EventArgs.IpPort.StartsWith(clientIp))
                .Select(p => false);

            connect
                .Merge(disconnect)
                .SubscribeOn(NewThreadScheduler.Default)
                .Subscribe(s => _connectionState.OnNext(s))
                .DisposeWith(_disposables);

            Observable
                .FromEventPattern<DataReceivedEventArgs>(h => _server.Events.DataReceived += h,
                    h => _server.Events.DataReceived -= h)
                .Select(p =>
                {
                    var d = p.EventArgs.Data;
                    return (d.Array ?? Array.Empty<byte>()).Skip(d.Offset).Take(d.Count).ToArray();
                })
                .SubscribeOn(new EventLoopScheduler())
                .Subscribe(b => _received.OnNext(b))
                .DisposeWith(_disposables);
        }

        public override async Task<bool> SendAsync(CommContext context)
        {
            var clientIpPort = _server.GetClients().FirstOrDefault(p => p.StartsWith(_clientIp));
            if (!string.IsNullOrWhiteSpace(clientIpPort))
            {
                var cmdStr = context.Command.Build();

                await _server.SendAsync(clientIpPort, cmdStr).ConfigureAwait(false);

                context.SendTime = DateTime.Now;
                _history.OnNext(context);

                return await Task.FromResult(true).ConfigureAwait(false);
            }

            return await Task.FromResult(false).ConfigureAwait(false);
        }

        public override async Task<bool> SendAsync(byte[] bytes)
        {
            var clientIpPort = _server.GetClients().FirstOrDefault(p => p.StartsWith(_clientIp));
            if (!string.IsNullOrWhiteSpace(clientIpPort))
            {
                await _server.SendAsync(clientIpPort, bytes).ConfigureAwait(false);
                await Task.Delay(50);
                return await Task.FromResult(true).ConfigureAwait(false);
            }

            return await Task.FromResult(false).ConfigureAwait(false);
        }

        public override async Task<bool> OpenAsync()
        {
            _server.Start();
            //await _server.StartAsync().ConfigureAwait(false);//持续监听导致不返回
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