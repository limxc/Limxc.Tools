using DynamicData;
using GodSharp.SerialPort;
using Limxc.Tools.DeviceComm.Entities;
using Limxc.Tools.DeviceComm.Extensions;
using Limxc.Tools.Extensions;
using System;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace Limxc.Tools.DeviceComm.Protocol
{
    public class SerialPortProtocol_GodSharp : IProtocol
    {
        private GodSerialPort _sp;

        private SourceList<CPContext> _msg;

        //private CompositeDisposable _disposables;

        public SerialPortProtocol_GodSharp()
        {
            _msg = new SourceList<CPContext>();

            //_disposables = new CompositeDisposable();

            //IsConnected = Observable.Empty<bool>();
            //Received = Observable.Empty<string>();
            //History = Observable.Empty<CPContext>();

            //Observable
            //    .Interval(TimeSpan.FromSeconds(0.5), TaskPoolScheduler.Default)
            //    .Where(_ => _sp != null)
            //    .DistinctUntilChanged()
            //    .FirstAsync()
            //    .Subscribe(__ =>
            //    {
            //        CreateObs();
            //    }
            //    ).DisposeWith(_disposables);

            _sp = new GodSerialPort(" ", 9600, 0);

            CreateObs();
        }

        private void CreateObs()
        {
            IsConnected = Observable.Defer(() =>
            {
                return Observable
                        .Interval(TimeSpan.FromSeconds(0.5), TaskPoolScheduler.Default)
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
                            _sp.OnData = (gs, data) => x.OnNext(data.ToHexStr());
                            return Disposable.Empty;
                        })
                        .Retry()
                        .Publish()
                        .RefCount();
            });

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

        public IObservable<bool> IsConnected { get; private set; }
        public IObservable<string> Received { get; private set; }
        public IObservable<CPContext> History { get; private set; }

        public void Dispose()
        {
            _msg?.Dispose();

            _sp?.Close();
            _sp = null;

            //_disposables?.Dispose();
        }

        public Task<bool> Send(CPContext cmd)
        {
            bool state = false;
            try
            {
                var cmdStr = cmd.ToCommand();
                state = _sp.WriteAsciiString(cmdStr) > 0;

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
                Disconnect();

                _sp.PortName = portName;
                _sp.BaudRate = baudRate;

                state = _sp.Open();
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
                state = _sp.Close();
            }
            catch (Exception e)
            {
            }
            return Task.FromResult(state);
        }
    }
}