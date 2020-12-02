using Limxc.Tools.DeviceComm.Entities;
using SimpleTcp;
using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;

namespace Limxc.Tools.DeviceComm.Protocol
{
    /// <summary>
    /// 用作下位机服务器,连接客户端一般只有一台
    /// </summary>
    public class TcpServerProtocol_SST : ITcpProtocol
    {
        private SimpleTcpServer _server;

        private ISubject<CPContext> _msg;

        public TcpServerProtocol_SST(string ip, int port)
        {
            _msg = new Subject<CPContext>();

            _server = new SimpleTcpServer(ip, port, false, null, null);

            var connect = Observable
                .FromEventPattern<ClientConnectedEventArgs>(h => _server.Events.ClientConnected += h, h => _server.Events.ClientConnected -= h)
                .Select(p => new ProcotolConnectionState()
                {
                    IpPort = p.EventArgs.IpPort,
                    IsConnected = true
                });

            var disconnect = Observable
                .FromEventPattern<ClientDisconnectedEventArgs>(h => _server.Events.ClientDisconnected += h, h => _server.Events.ClientDisconnected -= h)
                .Select(p => new ProcotolConnectionState()
                {
                    IpPort = p.EventArgs.IpPort,
                    IsConnected = false
                });

            ConnectionState = Observable
                .Merge(connect, disconnect)
                .Retry();

            Received = Observable.Defer(() =>
            {
                return Observable
                            .FromEventPattern<DataReceivedFromClientEventArgs>(h => _server.Events.DataReceived += h, h => _server.Events.DataReceived -= h)
                            .Select(p => p.EventArgs.Data);
            })
            //.Debug("receive")
            .Retry()
            .Publish()
            .RefCount();

            History = _msg.AsObservable();
        }

        public IObservable<ProcotolConnectionState> ConnectionState { get; private set; }
        public IObservable<byte[]> Received { get; private set; }
        public IObservable<CPContext> History { get; private set; }

        public void Dispose()
        {
            _msg?.OnCompleted();
            _msg = null;
        }

        /// <summary>
        /// 向最后成功连接的客户端发送指令
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        public async Task<bool> SendAsync(CPContext cmd)
        {
            bool state = false;
            try
            {
                var cmdStr = cmd.ToCommand();

                await _server.SendAsync(cmd.ClientId, cmdStr);
                if (state)
                {
                    cmd.SendTime = DateTime.Now;
                    _msg.OnNext(cmd);
                }
            }
            catch (Exception e)
            {
            }
            return await Task.FromResult(state);
        }

        public async Task<bool> OpenAsync()
        {
            bool state = false;
            try
            {
                await _server.StartAsync();
            }
            catch (Exception e)
            {
            }
            return await Task.FromResult(state); ;
        }

        public Task<bool> CloseAsync()
        {
            bool state = false;
            try
            {
                _server.Stop();
            }
            catch (Exception e)
            {
            }
            return Task.FromResult(state);
        }
    }
}