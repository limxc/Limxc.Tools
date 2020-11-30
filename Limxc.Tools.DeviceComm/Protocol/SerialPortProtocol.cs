using DynamicData;
using Limxc.Tools.DeviceComm.Entities;
using Limxc.Tools.DeviceComm.Extensions;
using Limxc.Tools.Extensions;
using Microsoft.Extensions.Logging;
using SerialPortLib;
using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading.Tasks;
using static SerialPortLib.SerialPortInput;

namespace Limxc.Tools.DeviceComm.Protocol
{
    public class SerialPortProtocol : IProtocol
    {
        private SerialPortInput _serialPort;

        private ILogger<SerialPortInput> _logger;

        private SourceList<CPContext> _msg;

        public SerialPortProtocol()
        {
            _msg = new SourceList<CPContext>();

            var loggerFactory = LoggerFactory.Create(builder => builder.AddDebug());
            _logger = loggerFactory.CreateLogger<SerialPortInput>();

            _serialPort = new SerialPortInput(_logger);

            IsConnected = Observable.FromEvent<ConnectionStatusChangedEventHandler, ConnectionStatusChangedEventArgs>
                    (
                        h => _serialPort.ConnectionStatusChanged += h,
                        h => _serialPort.ConnectionStatusChanged -= h
                    )
                .Select(p => p.Connected)
                .StartWith(false)
                .DistinctUntilChanged()
                .Publish()
                .RefCount();

            Received = Observable.FromEvent<MessageReceivedEventHandler, MessageReceivedEventArgs>
                    (
                        h => _serialPort.MessageReceived += h,
                        h => _serialPort.MessageReceived -= h
                    )
                .Select(p => p.Data.ToHexStr())
                .Publish()
                .RefCount();

            History = _msg
                .Connect()
                .SelectMany(p => p)
                .Select(p => p.Item.Current)
                .SelectMany(p =>
                {
                    if (p.TimeOut == 0 || string.IsNullOrWhiteSpace(p.Response.Template) || p.SendTime == null)
                        return Observable.Return(p);

                    var st = ((DateTime)p.SendTime).ToDateTimeOffset();
                    return Received
                             .Timestamp()
                             .SkipUntil(st)
                             .TakeUntil(st.AddMilliseconds(p.TimeOut))
                             .FirstAsync(t => p.Response.Template.IsMatch(t.Value))
                             .Select(r =>
                             {
                                 p.Response.Value = r.Value;
                                 p.ReceivedTime = r.Timestamp.LocalDateTime;
                                 return p;
                             })
                             .DefaultIfEmpty(p);
                })
                .SubscribeOn(TaskPoolScheduler.Default)
                .AsObservable();
        }

        public IObservable<bool> IsConnected { get; }
        public IObservable<string> Received { get; }
        public IObservable<CPContext> History { get; }

        public void Dispose()
        {
            _msg.Dispose();

            _serialPort.Disconnect();
            _serialPort = null;
            //_logger = null;
        }

        public Task<bool> Send(CPContext cmd)
        {
            bool state = false;
            try
            {
                var cmdStr = cmd.ToCommand();
                state = _serialPort.SendMessage(cmdStr.ToByte());
                if (state)
                {
                    cmd.SendTime = DateTime.Now;
                    _msg.Add(cmd);
                }
            }
            catch (Exception e)
            {
            }
            return Task.FromResult(state);
        }

        public Task<bool> Start(string portName, int baudRate)
        {
            bool state = false;
            try
            {
                _serialPort.SetPort(portName, baudRate);
                _serialPort.Connect();
                state = true;
            }
            catch (Exception e)
            {
            }
            return Task.FromResult(state);
        }

        public Task<bool> Stop()
        {
            bool state = false;
            try
            {
                _serialPort.Disconnect();
                state = true;
            }
            catch (Exception e)
            {
            }
            return Task.FromResult(state);
        }
    }
}