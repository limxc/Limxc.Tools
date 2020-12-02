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
    public class SerialPortProtocol_GS : IPortProtocol
    {
        private GodSerialPort _sp;

        private ISubject<CPContext> _msg;

        public SerialPortProtocol_GS(string portName, int baudRate)
        {
            _msg = new Subject<CPContext>();

            _sp = new GodSerialPort(portName, baudRate, 0);

            ConnectionState = Observable.Defer(() =>
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
                        .Create<byte[]>(x =>
                        {
                            _sp.UseDataReceived(true, (gs, data) =>
                            {
                                if (data != null && data.Length > 0)
                                    x.OnNext(data);
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

        public IObservable<bool> ConnectionState { get; private set; }
        public IObservable<byte[]> Received { get; private set; }
        public IObservable<CPContext> History { get; private set; }

        public void Dispose()
        {
            _msg?.OnCompleted();
            _msg = null;

            _sp?.Close();
            _sp = null;
        }

        /// <summary>
        /// 使用GodSharp.SerialPort: 响应时间建议>128ms
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        public Task<bool> SendAsync(CPContext cmd)
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

        public async Task<bool> OpenAsync()
        {
            bool state = false;
            try
            {
                if (_sp.IsOpen)
                    await CloseAsync();

                state = _sp.Open();
            }
            catch (Exception e)
            {
            }
            return await Task.FromResult(state);
        }

        public Task<bool> CloseAsync()
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