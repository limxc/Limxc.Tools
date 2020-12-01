using Limxc.Tools.DeviceComm.Entities;
using Limxc.Tools.DeviceComm.Extensions;
using Limxc.Tools.DeviceComm.Utils;
using Limxc.Tools.Extensions;
using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using static Limxc.Tools.DeviceComm.Utils.SerialPortStreamHelper;

namespace Limxc.Tools.DeviceComm.Protocol
{
    public class SerialPortProtocol_SPS : IProtocol
    {
        private SerialPortStreamHelper _sp;

        private ISubject<CPContext> _msg;

        public SerialPortProtocol_SPS()
        {
            _msg = new Subject<CPContext>();

            _sp = new SerialPortStreamHelper();

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

            Received = Observable
                .FromEventPattern<DataReceivedEventHandle, byte[]>(h => _sp.ReceivedEvent += h, h => _sp.ReceivedEvent -= h)
                .Where(p => p.EventArgs != null && p.EventArgs.Length > 0)
                .Select(p => p.EventArgs.ToHexStrFromChar())
                .Retry()
                .Publish()
                .RefCount()
                //.Debug("receive")
                ;

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
            _sp.Dispose();
            _sp = null;
        }

        /// <summary>
        /// 使用SerialPortStream: 响应延时建议>256ms
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        public Task<bool> Send(CPContext cmd)
        {
            bool state = false;
            try
            {
                var cmdStr = cmd.ToCommand();
                _sp.Write(cmdStr);
                state = true;
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

                state = _sp.Open(portName, baudRate);
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
                _sp.Close();
                state = true;
            }
            catch (Exception e)
            {
            }
            return Task.FromResult(state);
        }
    }
}