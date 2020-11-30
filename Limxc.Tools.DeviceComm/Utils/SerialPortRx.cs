using Limxc.Tools.DeviceComm.Extensions;
using System;
using System.IO.Ports;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Limxc.Tools.DeviceComm.Utils
{
    public class SerialPortRx
    {
        internal readonly ISubject<bool> isOpen = new ReplaySubject<bool>(1);
        private readonly ISubject<char> dataReceived = new Subject<char>();
        private readonly ISubject<Exception> errors = new Subject<Exception>();
        private readonly ISubject<Tuple<byte[], int, int>> writeByte = new Subject<Tuple<byte[], int, int>>();
        private readonly ISubject<string> writeString = new Subject<string>();
        private CompositeDisposable _disposables = new CompositeDisposable();

        public SerialPortRx()
        {
        }

        public SerialPortRx(string port, int baudRate)
        {
            PortName = port;
            BaudRate = baudRate;
        }

        #region 参数

        public string PortName { get; set; } = "COM1";
        public int BaudRate { get; set; } = 9600;
        public StopBits StopBits { get; set; } = StopBits.One;
        public Parity Parity { get; set; } = Parity.None;
        public int DataBits { get; set; } = 8;

        public int ReadTimeout { get; set; } = -1;
        public int WriteTimeout { get; set; } = -1;
        public Handshake Handshake { get; set; } = Handshake.None;
        public Encoding Encoding { get; set; } = new ASCIIEncoding();

        public bool IsOpen { get; private set; }

        public bool IsDisposed { get; private set; } = false;

        #endregion 参数

        public IObservable<char> DataReceived => dataReceived.Retry().Publish().RefCount();
        public IObservable<Exception> ErrorReceived => errors.Distinct(ex => ex.Message).Retry().Publish().RefCount();
        public IObservable<bool> IsOpenObservable => isOpen.DistinctUntilChanged();

        private IObservable<Unit> Connect
            => Observable.Create<Unit>(obs =>
            {
                var dis = new CompositeDisposable();

                if (!SerialPort.GetPortNames().Any(name => name.Equals(PortName)))
                {
                    obs.OnError(new Exception($"端口不存在. {PortName}"));
                }
                else
                {
                    var port = new SerialPort(PortName, BaudRate, Parity, DataBits, StopBits);
                    dis.Add(port);
                    port.Close();
                    port.Handshake = Handshake;
                    port.ReadTimeout = ReadTimeout;
                    port.WriteTimeout = WriteTimeout;
                    port.Encoding = Encoding;
                    try
                    {
                        port.Open();
                    }
                    catch (Exception ex)
                    {
                        errors.OnNext(ex);
                        obs.OnCompleted();
                    }
                    isOpen.OnNext(port.IsOpen);
                    IsOpen = port.IsOpen;

                    if (IsOpen)
                    {
                        port.DiscardInBuffer();
                        port.DiscardOutBuffer();
                    }
                    Thread.Sleep(100);

                    port.ErrorReceivedObserver().Subscribe(e => obs.OnError(new Exception(e.EventArgs.EventType.ToString()))).DisposeWith(dis);

                    port.DataReceivedObserver().Select(_ => port.ReadExisting()).SelectMany(p => p)
                        .Subscribe(dataReceived.OnNext, obs.OnError).DisposeWith(dis); ;

                    writeString.Subscribe(x =>
                    {
                        try
                        { port?.Write(x); }
                        catch (Exception ex)
                        { obs.OnError(ex); }
                    }, obs.OnError).DisposeWith(dis);

                    writeByte.Subscribe(x =>
                    {
                        try { port?.Write(x.Item1, x.Item2, x.Item3); }
                        catch (Exception ex)
                        {
                            obs.OnError(ex);
                        }
                    }, obs.OnError).DisposeWith(dis);
                }
                return Disposable.Create(() =>
                {
                    IsOpen = false;
                    isOpen.OnNext(false);
                    dis.Dispose();
                });
            }).OnErrorRetry((Exception ex) => errors.OnNext(ex)).Publish().RefCount();

        public static IObservable<string[]> PortNames(int pollInterval = 500, int pollLimit = 0)
            => Observable.Create<string[]>(obs =>
                {
                    string[] compare = null;
                    var numberOfPolls = 0;
                    return Observable.Interval(TimeSpan.FromMilliseconds(pollInterval)).Subscribe(_ =>
                    {
                        var compareNew = SerialPort.GetPortNames();
                        if (compareNew.Length == 0)
                        {
                            compareNew = new string[] { "NoPorts" };
                        }

                        if (compare == null)
                        {
                            compare = compareNew;
                            obs.OnNext(compareNew);
                        }
                        if (string.Concat(compare) != string.Concat(compareNew))
                        {
                            obs.OnNext(compareNew);
                            compare = compareNew;
                        }
                        if (numberOfPolls > pollLimit)
                        {
                            obs.OnCompleted();
                        }
                        if (pollLimit > 0 && numberOfPolls < pollLimit)
                        {
                            numberOfPolls++;
                        }
                    });
                })
                .Retry().Publish().RefCount();

        public Task Open()
        {
            return _disposables?.Count == 0 ? Task.Run(() => Connect.Subscribe().DisposeWith(_disposables)) : Task.CompletedTask;
        }

        public void Close()
        {
            _disposables?.Dispose();
        }

        public void Write(string text) => writeString?.OnNext(text);

        public void Write(byte[] byteArray) => writeByte?.OnNext(new Tuple<byte[], int, int>(byteArray, 0, byteArray.Length));

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing">
        /// <c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only
        /// unmanaged resources.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing)
                {
                    _disposables?.Dispose();
                }

                IsDisposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
    }
}