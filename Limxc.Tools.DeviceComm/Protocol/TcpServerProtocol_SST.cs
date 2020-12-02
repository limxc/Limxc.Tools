using Limxc.Tools.DeviceComm.Entities;
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
    public class TcpServerProtocol_SST : IPortProtocol
    {
        private SimpleTcpServer _server;

        private ISubject<CPContext> _msg;

        private string _clientIpPort;

        public TcpServerProtocol_SST(string ipPort, string allowedIp)
        {
            _msg = new Subject<CPContext>();

            _server = new SimpleTcpServer(ipPort);

            var connect = Observable
                            .FromEventPattern<ClientConnectedEventArgs>(h => _server.Events.ClientConnected += h, h => _server.Events.ClientConnected -= h)
                            .Where(p => p.EventArgs.IpPort.StartsWith(allowedIp))
                            .Select(p => (p.EventArgs.IpPort, true));

            var disconnect = Observable
                .FromEventPattern<ClientDisconnectedEventArgs>(h => _server.Events.ClientDisconnected += h, h => _server.Events.ClientDisconnected -= h)
                .Where(p => p.EventArgs.IpPort.StartsWith(allowedIp))
                .Select(p => (p.EventArgs.IpPort, false));

            ConnectionState = Observable
                .Merge(connect, disconnect)
                .Do(p => _clientIpPort = p.IpPort)
                .Select(p => p.Item2)
                .Retry();

            Received = Observable.Defer(() =>
            {
                return Observable
                            .FromEventPattern<DataReceivedFromClientEventArgs>(h => _server.Events.DataReceived += h, h => _server.Events.DataReceived -= h)
                            .Where(p => p.EventArgs.IpPort.StartsWith(allowedIp))
                            .Select(p => p.EventArgs.Data);
            })
            .Retry()
            .Publish()
            .RefCount();

            History = _msg.AsObservable();
        }

        public IObservable<bool> ConnectionState { get; private set; }
        public IObservable<byte[]> Received { get; private set; }
        public IObservable<CPContext> History { get; private set; }

        public void Dispose()
        {
            _msg?.OnCompleted();
            _msg = null;
        }

        public async Task<bool> SendAsync(CPContext cmd)
        {
            bool state = false;
            try
            {
                var cmdStr = cmd.ToCommand();

                await _server.SendAsync(_clientIpPort, cmdStr);
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