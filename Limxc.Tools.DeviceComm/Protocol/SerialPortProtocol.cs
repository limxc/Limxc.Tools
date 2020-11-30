﻿using DynamicData;
using Limxc.Tools.DeviceComm.Entities;
using Limxc.Tools.DeviceComm.Extensions;
using Limxc.Tools.DeviceComm.Utils;
using Limxc.Tools.Extensions;
using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading.Tasks;
using static Limxc.Tools.DeviceComm.Utils.SerialPortLibHelper;

namespace Limxc.Tools.DeviceComm.Protocol
{
    public class SerialPortProtocol : IProtocol
    {
        private SerialPortLibHelper _sp;

        private SourceList<CPContext> _msg;

        public SerialPortProtocol()
        {
            _msg = new SourceList<CPContext>();

            _sp = new SerialPortLibHelper();

            IsConnected = Observable.FromEvent<ConnectionStatusChangedEventHandler, ConnectionStatusChangedEventArgs>
                    (
                        h => _sp.ConnectionStatusChanged += h,
                        h => _sp.ConnectionStatusChanged -= h
                    )
                .Select(p => p.Connected)
                .StartWith(false)
                .DistinctUntilChanged()
                .Retry()
                .Publish()
                .RefCount();

            Received = Observable.FromEvent<MessageReceivedEventHandler, MessageReceivedEventArgs>
                    (
                        h => _sp.MessageReceived += h,
                        h => _sp.MessageReceived -= h
                    )
                .Select(p => p.Data.ToHexStr())
                .Retry()
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

            _sp.Disconnect();
            _sp = null;
            //_logger = null;
        }

        public Task<bool> Send(CPContext cmd)
        {
            bool state = false;
            try
            {
                var cmdStr = cmd.ToCommand();
                state = _sp.SendMessage(cmdStr.ToByte());
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

        public Task<bool> Connect(string portName, int baudRate)
        {
            bool state = false;
            try
            {
                _sp.SetPort(portName, baudRate);
                _sp.Connect();
                state = true;
            }
            catch (Exception e)
            {
            }
            return Task.FromResult(state);
        }

        public Task<bool> Disconnect()
        {
            bool state = false;
            try
            {
                _sp.Disconnect();
                state = true;
            }
            catch (Exception e)
            {
            }
            return Task.FromResult(state);
        }
    }
}