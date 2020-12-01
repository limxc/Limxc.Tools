using Limxc.Tools.DeviceComm.Entities;
using Limxc.Tools.DeviceComm.Extensions;
using Limxc.Tools.Extensions;
using SimpleTcp;
using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;

namespace Limxc.Tools.DeviceComm.Protocol
{
    public class TcpServerProtocol_SST : ITcpProtocol
    {
        private SimpleTcpServer _sp;

        private ISubject<CPContext> _msg;

        public TcpServerProtocol_SST(string ip, int port)
        {
            _msg = new Subject<CPContext>();

            _sp = new SimpleTcpServer(ip, port, false, null, null);

            var connect = Observable
                .FromEventPattern<ClientConnectedEventArgs>(h => _sp.Events.ClientConnected += h, h => _sp.Events.ClientConnected -= h)
                .Select(p => new ProcotolConnectionState()
                {
                    IpPort = p.EventArgs.IpPort,
                    IsConnected = true
                });

            var disconnect = Observable
                .FromEventPattern<ClientDisconnectedEventArgs>(h => _sp.Events.ClientDisconnected += h, h => _sp.Events.ClientDisconnected -= h)
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
                            .FromEventPattern<DataReceivedFromClientEventArgs>(h => _sp.Events.DataReceived += h, h => _sp.Events.DataReceived -= h)
                            .Select(p => p.EventArgs.Data);
            })
            //.Debug("receive")
            .Retry()
            .Publish()
            .RefCount();

            History = _msg
                //.Debug("send")
                .SelectMany(p =>
                {
                    if (p.TimeOut == 0 || string.IsNullOrWhiteSpace(p.Response.Template) || p.SendTime == null)
                        return Observable.Return(p);

                    var st = ((DateTime)p.SendTime).ToDateTimeOffset();
                    return Received
                             .Select(d => d.ToHexStr())
                             .Timestamp()
                             .SkipUntil(st)
                             .TakeUntil(st.AddMilliseconds(p.TimeOut))
                             .FirstOrDefaultAsync(t =>
                             {
                                 return p.Response.Template.IsMatch(t.Value);
                             })
                             .Where(r => r.Value != null)
                             .Select(r =>
                             {
                                 p.Response.Value = r.Value;
                                 p.ReceivedTime = r.Timestamp.LocalDateTime;
                                 return p;
                             })
                             .DefaultIfEmpty(p)
                             //.Debug("merge")
                             ;
                })
                .SubscribeOn(TaskPoolScheduler.Default)
                .AsObservable();
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
        /// 使用GodSharp.SerialPort: 响应时间建议>128ms
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        public Task<bool> Send(CPContext cmd)
        {
            bool state = false;
            try
            {
                var cmdStr = cmd.ToCommand();
                //state = _sp.WriteHexString(cmdStr) > 0;

                //_sp.Send(ip, b);

                if (state)
                {
                    cmd.SendTime = DateTime.Now;
                    _msg.OnNext(cmd);
                }
            }
            catch (Exception e)
            {
            }
            return Task.FromResult(state);
        }

        public async Task<bool> Connect()
        {
            bool state = false;
            try
            {
                //if (_sp.IsOpen)
                //    await Disconnect();

                //_sp.PortName = portName;
                //_sp.BaudRate = baudRate;

                //state = _sp.Open();
            }
            catch (Exception e)
            {
            }
            return await Task.FromResult(state);
        }

        public Task<bool> Disconnect()
        {
            bool state = false;
            try
            {
                //state = _sp.Close();
            }
            catch (Exception e)
            {
            }
            return Task.FromResult(state);
        }
    }
}