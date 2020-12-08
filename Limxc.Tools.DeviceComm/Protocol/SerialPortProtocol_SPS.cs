using Limxc.Tools.DeviceComm.Entities;
using Limxc.Tools.DeviceComm.Extensions;
using Limxc.Tools.DeviceComm.Utils;
using System;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using static Limxc.Tools.DeviceComm.Utils.SerialPortStreamHelper;

namespace Limxc.Tools.DeviceComm.Protocol
{
    public class SerialPortProtocol_SPS : IProtocol
    {
        private SerialPortStreamHelper _sp;
        private readonly string _portName;
        private readonly int _baudRate;

        private ISubject<CPContext> _msg;

        public SerialPortProtocol_SPS(string portName, int baudRate)
        {
            _portName = portName;
            _baudRate = baudRate;

            _msg = new Subject<CPContext>();

            _sp = new SerialPortStreamHelper();

            ConnectionState = Observable
                            .Interval(TimeSpan.FromSeconds(0.1))
                            .Select(_ => _sp.IsOpen)
                            .StartWith(false)
                            .DistinctUntilChanged()
                            .Retry()
                            .Publish()
                            .RefCount();

            Received = Observable
                            .FromEventPattern<DataReceivedEventHandle, byte[]>(h => _sp.ReceivedEvent += h, h => _sp.ReceivedEvent -= h)
                            .Where(p => p.EventArgs != null && p.EventArgs.Length > 0)
                            .Select(p => p.EventArgs)
                            .Retry()
                            .Publish()
                            .RefCount()
                            //.Debug("receive")
                            ;

            History = _msg.AsObservable()
                            //.Debug("send")
                            .FindResponse(Received, b => b.ToHexStrFromChar())
                            //.Debug("prase received")
                            ;
        }

        public IObservable<bool> ConnectionState { get; private set; }
        public IObservable<byte[]> Received { get; private set; }
        public IObservable<CPContext> History { get; private set; }

        public void CleanUp()
        {
            _msg?.OnCompleted();
            _msg = null;

            _sp?.Close();
            _sp.CleanUp();
            _sp = null;
        }

        /// <summary>
        /// 使用SerialPortStream: 响应时间建议>256ms
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        public Task<bool> SendAsync(CPContext cmd)
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

                state = _sp.Open(_portName, _baudRate);
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
                _sp.Close();
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