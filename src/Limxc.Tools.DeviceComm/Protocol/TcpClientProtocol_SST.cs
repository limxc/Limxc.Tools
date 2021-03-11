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
    public class TcpClientProtocol_SST : IProtocol
    {
        private readonly string _serverIpPort;
        private SimpleTcpClient _client;

        private ISubject<CPContext> _msg;
        private bool disposedValue;

        public TcpClientProtocol_SST(string serverIpPort)
        {
            if (!serverIpPort.CheckIpPort())
                throw new ArgumentException($"IpPort Error : {serverIpPort}");

            _serverIpPort = serverIpPort;

            _msg = new Subject<CPContext>();

            _client = new SimpleTcpClient(_serverIpPort);

            var connect = Observable
                    .FromEventPattern<ClientConnectedEventArgs>(h => _client.Events.Connected += h,
                        h => _client.Events.Connected -= h)
                    .Select(_ => true)
                ;

            var disconnect = Observable
                .FromEventPattern<ClientDisconnectedEventArgs>(h => _client.Events.Disconnected += h,
                    h => _client.Events.Disconnected -= h)
                .Select(_ => false);

            ConnectionState = Observable.Defer(() =>
            {
                return connect
                    .Merge(disconnect)
                    .Retry();
            });

            Received = Observable
                    .FromEventPattern<DataReceivedEventArgs>(h => _client.Events.DataReceived += h,
                        h => _client.Events.DataReceived -= h)
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
            var cmdStr = context.Command.Build();

            await _client.SendAsync(cmdStr).ConfigureAwait(false);

            context.SendTime = DateTime.Now;
            _msg.OnNext(context);

            return await Task.FromResult(true).ConfigureAwait(false);
        }

        public async Task<bool> SendAsync(byte[] bytes)
        {
            await _client.SendAsync(bytes).ConfigureAwait(false);

            return await Task.FromResult(true).ConfigureAwait(false);
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
                _client?.Disconnect();
                _client?.Dispose();
                // TODO: 将大型字段设置为 null
                _msg = null;
                _client = null;
                disposedValue = true;
            }
        }

        // TODO: 仅当“Dispose(bool disposing)”拥有用于释放未托管资源的代码时才替代终结器
        ~TcpClientProtocol_SST()
        {
            // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
            Dispose(false);
        }
    }
}