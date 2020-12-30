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
    public class TcpServerProtocol_SST : IProtocol
    {
        private SimpleTcpServer _server;

        private ISubject<CPContext> _msg;

        private readonly string _ipPort;
        private readonly string _clientIpPort;

        public TcpServerProtocol_SST(string ipPort, string clientIpPort)
        {
            if (!ipPort.CheckIpPort())
                throw new ArgumentException($"IpPort Error : {ipPort}");

            _ipPort = ipPort;
            _clientIpPort = clientIpPort;

            _msg = new Subject<CPContext>();

            _server = new SimpleTcpServer(_ipPort);

            var connect = Observable
                .FromEventPattern<ClientConnectedEventArgs>(h => _server.Events.ClientConnected += h, h => _server.Events.ClientConnected -= h)
                .Select(p => (p.EventArgs.IpPort, true));

            var disconnect = Observable
                .FromEventPattern<ClientDisconnectedEventArgs>(h => _server.Events.ClientDisconnected += h, h => _server.Events.ClientDisconnected -= h)
                .Select(p => (p.EventArgs.IpPort, false));

            ConnectionState = Observable
                    .Merge(connect, disconnect)
                    .Select(p => p.Item2)
                    .Debug(_ipPort)
                    .Retry();

            Received = Observable
                            .FromEventPattern<DataReceivedEventArgs>(h => _server.Events.DataReceived += h, h => _server.Events.DataReceived -= h)
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
            _server?.Stop();
            _server?.Dispose();

            _msg?.OnCompleted();
            _msg = null;
        }

        public async Task<bool> SendAsync(CPContext context)
        {
            if (_clientIpPort.CheckIpPort())
            {
                var cmdStr = context.Command.Build();

                await _server.SendAsync(_clientIpPort, cmdStr).ConfigureAwait(false);

                context.SendTime = DateTime.Now;
                _msg.OnNext(context);

                return await Task.FromResult(true);
            }
            else
            {
                return await Task.FromResult(false);
            }
        }

        public async Task<bool> SendAsync(byte[] bytes)
        {
            if (_clientIpPort.CheckIpPort())
            {
                await _server.SendAsync(_clientIpPort, bytes).ConfigureAwait(false);
                return await Task.FromResult(true);
            }
            else
            {
                return await Task.FromResult(false);
            }
        }

        public async Task<bool> OpenAsync()
        {
            await _server.StartAsync();
            return await Task.FromResult(true);
        }

        public Task<bool> CloseAsync()
        {
            _server.Stop();
            return Task.FromResult(true);
        }
    }
}