using GodSharp.SerialPort;
using Limxc.Tools.DeviceComm.Entities;
using Limxc.Tools.DeviceComm.Extensions;
using System;
using System.Diagnostics;
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
        private readonly string _portName;
        private readonly int _baudRate;

        public SerialPortProtocol_GS(string portName, int baudRate)
        {
            _portName = portName;
            _baudRate = baudRate;

            _msg = new Subject<CPContext>();

            _sp = new GodSerialPort(_portName, _baudRate, 0);

            ConnectionState = Observable
                            .Interval(TimeSpan.FromSeconds(0.1))
                            .Select(_ => _sp.IsOpen)
                            .StartWith(false)
                            .DistinctUntilChanged()
                            .Retry()
                            .Publish()
                            .RefCount();

            Received = Observable
                            .Create<byte[]>(x =>
                            {
                                _sp.UseDataReceived(true, (gs, data) =>
                                {
                                    if (data != null && data.Length > 0)
                                        x.OnNext(data);
                                });
                                return Disposable.Empty;
                            })
                            .Retry()
                            .Publish()
                            .RefCount()
                             //.Debug("receive")
                             ;

            History = _msg.AsObservable()
                            //.Debug("send")
                            .FindResponse(Received)
                            //.Debug("prase received")
                            ;
        }

        public IObservable<bool> ConnectionState { get; }
        public IObservable<byte[]> Received { get; }
        public IObservable<CPContext> History { get; }

        public void CleanUp()
        {
            _msg?.OnCompleted();
            _msg = null;

            _sp?.Close();
            _sp = null;
        }

        /// <summary>
        /// 使用GodSharp.SerialPort: 响应时间建议>128ms
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public Task<bool> SendAsync(CPContext context)
        {
            bool state = false;
            try
            {
                var cmdStr = context.Command.Build();
                state = _sp.WriteHexString(cmdStr) > 0;

                if (state)
                {
                    context.SendTime = DateTime.Now;
                    _msg.OnNext(context);
                }
            }
            catch (Exception e)
            {
                if (Debugger.IsAttached)
                    throw e;
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
                if (Debugger.IsAttached)
                    throw e;
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
                if (Debugger.IsAttached)
                    throw e;
            }
            return Task.FromResult(state);
        }
    }
}