using GodSharp.SerialPort;
using Limxc.Tools.DeviceComm.Entities;
using Limxc.Tools.DeviceComm.Extensions;
using Limxc.Tools.Extensions;
using System;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;

namespace Limxc.Tools.DeviceComm.Protocol
{
    public class SerialPortProtocol_GS : IProtocol
    {
        private GodSerialPort _sp;

        private ISubject<CPContext> _msg;

        public SerialPortProtocol_GS()
        {
            _msg = new Subject<CPContext>();

            _sp = new GodSerialPort(" ", 9600, 0);

            IsConnected = Observable.Defer(() =>
            {
                return Observable
                        .Interval(TimeSpan.FromSeconds(0.1), TaskPoolScheduler.Default)
                        .Select(_ => _sp.IsOpen)
                        .StartWith(false)
                        .DistinctUntilChanged()
                        .Retry()
                        .Publish()
                        .RefCount();
            });

            Received = Observable.Defer(() =>
            {
                return Observable
                        .Create<string>(x =>
                        {
                            _sp.UseDataReceived(true, (gs, data) =>
                            {
                                if (data != null && data.Length > 0)
                                    x.OnNext(data.ToHexStr());
                            });
                            return Disposable.Empty;
                        });
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

        public IObservable<bool> IsConnected { get; private set; }
        public IObservable<string> Received { get; private set; }
        public IObservable<CPContext> History { get; private set; }

        public void Dispose()
        {
            _msg?.OnCompleted();
            _msg = null;

            _sp?.Close();
            _sp = null;
        }

        /// <summary>
        /// 使用GodSharp.SerialPort: 响应延时建议>128ms
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        public Task<bool> Send(CPContext cmd)
        {
            bool state = false;
            try
            { 
                var cmdStr = cmd.ToCommand();
                state = _sp.WriteHexString(cmdStr) > 0;

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

        public async Task<bool> Connect(string portName, int baudRate)
        {
            bool state = false;
            try
            {
                if (_sp.IsOpen)
                    await Disconnect();

                _sp.PortName = portName;
                _sp.BaudRate = baudRate;

                state = _sp.Open();
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
                state = _sp.Close();
            }
            catch (Exception e)
            {
            }
            return Task.FromResult(state);
        }
    }
}