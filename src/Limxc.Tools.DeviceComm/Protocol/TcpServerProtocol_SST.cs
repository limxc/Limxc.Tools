using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Limxc.Tools.Entities.DevComm;
using Limxc.Tools.Extensions;
using Limxc.Tools.Extensions.DevComm;
using SimpleTcp;

namespace Limxc.Tools.DeviceComm.Protocol
{
    /// <summary>
    ///     用作下位机服务器,连接客户端一般只有一台
    /// </summary>
    public class TcpServerProtocol_SST : IProtocol
    {
        private readonly string _clientIpPort;
        private readonly string _ipPort;

        private ISubject<CPContext> _msg;
        private SimpleTcpServer _server;
        private bool disposedValue;

        public TcpServerProtocol_SST(string ipPort, string clientIpPort)
        {
            if (!ipPort.CheckIpPort())
                throw new ArgumentException($"IpPort Error : {ipPort}");

            _ipPort = ipPort;
            _clientIpPort = clientIpPort;

            _msg = new Subject<CPContext>();

            _server = new SimpleTcpServer(_ipPort);

            var connect = Observable
                .FromEventPattern<ClientConnectedEventArgs>(h => _server.Events.ClientConnected += h,
                    h => _server.Events.ClientConnected -= h)
                .Select(p => (p.EventArgs.IpPort, true));

            var disconnect = Observable
                .FromEventPattern<ClientDisconnectedEventArgs>(h => _server.Events.ClientDisconnected += h,
                    h => _server.Events.ClientDisconnected -= h)
                .Select(p => (p.EventArgs.IpPort, false));

            ConnectionState = Observable.Defer(() =>
            {
                return connect
                    .Merge(disconnect)
                    .Select(p => p.Item2)
                    //.Debug(_ipPort)
                    .Retry();
            });

            Received = Observable
                    .FromEventPattern<DataReceivedEventArgs>(h => _server.Events.DataReceived += h,
                        h => _server.Events.DataReceived -= h)
                    .Select(p => p.EventArgs.Data)
                    .Retry()
                    .Publish()
                    .RefCount()
                //.Debug("receive")
                ;

            History = Observable.Defer(() =>
            {
                return _msg.AsObservable()
                        //.Debug("send")
                        .FindResponse(Received)
                    //.Debug("prase received")
                    ;
            });
        }

        public IObservable<bool> ConnectionState { get; }
        public IObservable<byte[]> Received { get; }
        public IObservable<CPContext> History { get; }

        public async Task<bool> SendAsync(CPContext context)
        {
            if (_clientIpPort.CheckIpPort())
            {
                var cmdStr = context.Command.Build();

                await _server.SendAsync(_clientIpPort, cmdStr).ConfigureAwait(false);

                context.SendTime = DateTime.Now;
                _msg.OnNext(context);

                return await Task.FromResult(true).ConfigureAwait(false);
            }

            return await Task.FromResult(false).ConfigureAwait(false);
        }

        public async Task<bool> SendAsync(byte[] bytes)
        {
            if (_clientIpPort.CheckIpPort())
            {
                await _server.SendAsync(_clientIpPort, bytes).ConfigureAwait(false);
                return await Task.FromResult(true).ConfigureAwait(false);
            }

            return await Task.FromResult(false).ConfigureAwait(false);
        }

        public async Task<bool> OpenAsync()
        {
            await _server.StartAsync().ConfigureAwait(false);
            return await Task.FromResult(true).ConfigureAwait(false);
        }

        public Task<bool> CloseAsync()
        {
            _server.Stop();
            return Task.FromResult(true);
        }

        public void Dispose()
        {
            // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                    // TODO: 释放托管状态(托管对象)
                    _msg?.OnCompleted();

                // TODO: 释放未托管的资源(未托管的对象)并替代终结器
                _server?.Stop();
                _server?.Dispose();
                // TODO: 将大型字段设置为 null
                _msg = null;
                _server = null;
                disposedValue = true;
            }
        }

        // TODO: 仅当“Dispose(bool disposing)”拥有用于释放未托管资源的代码时才替代终结器
        ~TcpServerProtocol_SST()
        {
            // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
            Dispose(false);
        }
    }
}