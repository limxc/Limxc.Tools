using Limxc.Tools.DeviceComm.Entities;
using Limxc.Tools.DeviceComm.Extensions;
using SimpleTcp;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Limxc.Tools.Extensions;

namespace Limxc.Tools.DeviceComm.Protocol
{
    /// <summary>
    /// 用作下位机服务器,连接客户端一般只有一台
    /// </summary>
    public class TcpClientProtocol_SST : IPortProtocol
    {
        private SimpleTcpClient _server;

        private ISubject<CPContext> _msg;

        private readonly string _serverIpPort;

        public TcpClientProtocol_SST(string serverIpPort)
        {
            if (!serverIpPort.CheckIpPort())
                throw new ArgumentException($"IpPort Error : {serverIpPort}");

            _serverIpPort = serverIpPort;

            _msg = new Subject<CPContext>();

            _server = new SimpleTcpClient(_serverIpPort);

            var connect = Observable
                            .FromEventPattern(h => _server.Events.Connected += h, h => _server.Events.Connected -= h)
                            .Select(_ => true)
                             ;

            var disconnect = Observable
                .FromEventPattern(h => _server.Events.Disconnected += h, h => _server.Events.Disconnected -= h)
                .Select(_ => false);

            ConnectionState = Observable
                .Merge(connect, disconnect)
                .Retry();

            Received = Observable.Defer(() =>
            {
                return Observable
                            .FromEventPattern<DataReceivedFromServerEventArgs>(h => _server.Events.DataReceived += h, h => _server.Events.DataReceived -= h)
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
                var cmdStr = cmd.ToCommand();

                await _server.SendAsync(cmdStr);
                state = true;

                cmd.SendTime = DateTime.Now;
                _msg.OnNext(cmd);
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
                _server.Connect();
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
                _server.Disconnect();
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