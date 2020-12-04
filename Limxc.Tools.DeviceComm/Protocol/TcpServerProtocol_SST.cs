using Limxc.Tools.DeviceComm.Entities;
using Limxc.Tools.DeviceComm.Extensions;
using Limxc.Tools.Extensions;
using SimpleTcp;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Concurrency;
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

            ConnectionState = Observable.Defer(() =>
            {
                var connect = Observable
                    .FromEventPattern<ClientConnectedEventArgs>(h => _server.Events.ClientConnected += h, h => _server.Events.ClientConnected -= h) 
                    .Select(p => (p.EventArgs.IpPort, true));

                var disconnect = Observable
                    .FromEventPattern<ClientDisconnectedEventArgs>(h => _server.Events.ClientDisconnected += h, h => _server.Events.ClientDisconnected -= h) 
                    .Select(p => (p.EventArgs.IpPort, false));

                return Observable
                    .Merge(connect, disconnect)
                    .Select(p => p.Item2)
                    .Debug(_ipPort)
                    .Retry();
            });

            Received = Observable.Defer(() =>
            {
                return Observable
                            .FromEventPattern<DataReceivedFromClientEventArgs>(h => _server.Events.DataReceived += h, h => _server.Events.DataReceived -= h)
                            .Select(p => p.EventArgs.Data)
                            .Retry()
                            .Publish()
                            .RefCount()
                            //.Debug("receive")
                            ;
            });

            History = Observable.Defer(() =>
            {
                return _msg.AsObservable()
                            //.Debug("send")
                            .FindResponse(Received)
                            //.Debug("prase received")
                            .SubscribeOn(TaskPoolScheduler.Default);
            });
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
                if (_clientIpPort.CheckIpPort())
                {
                    var cmdStr = cmd.ToCommand();

                    await _server.SendAsync(_clientIpPort, cmdStr);

                    state = true;

                    cmd.SendTime = DateTime.Now;
                    _msg.OnNext(cmd);
                }
            }
            catch (Exception e)
            {
                if (Debugger.IsAttached)
                    throw e;
            }
            return await Task.FromResult(state);
        }

        public async Task<bool> OpenAsync()
        {
            bool state = false;
            try
            {
                await _server.StartAsync();
                state = true;
            }
            catch (Exception e)
            {
                if (Debugger.IsAttached)
                    throw e;
            }
            return await Task.FromResult(state); ;
        }

        public Task<bool> CloseAsync()
        {
            bool state = false;
            try
            {
                _server.Stop();
                state = true;
            }
            catch (Exception e)
            {
                if (Debugger.IsAttached)
                    throw e;
            }
            return Task.FromResult(state);
        }
    }
}