using System;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Limxc.Tools.Extensions;
using Limxc.Tools.Extensions.Communication;

namespace Limxc.Tools.SerialPort
{
    public class SerialPortServiceSimulator : ISerialPortService
    {
        private readonly Subject<bool> _connectionState = new Subject<bool>();
        private readonly CompositeDisposable _initDisposables = new CompositeDisposable();
        private readonly Subject<string> _log = new Subject<string>();

        private readonly int _lostInterval;
        private readonly Subject<byte[]> _received = new Subject<byte[]>();
        private readonly int _sendDelay;

        private int _sendIndex;

        public SerialPortServiceSimulator(int lostInterval = 0, int sendDelay = 100)
        {
            _lostInterval = lostInterval;
            _sendDelay = sendDelay;

            Observable
                .Interval(TimeSpan.FromSeconds(1))
                .Select(_ => IsConnected)
                .SubscribeOn(NewThreadScheduler.Default)
                .Subscribe(s => _connectionState.OnNext(s))
                .DisposeWith(_initDisposables);

            ConnectionState = Observable.Defer(() =>
                _connectionState.StartWith(false).AsObservable().Publish().RefCount());
            Received = Observable.Defer(() => _received.AsObservable().Publish().RefCount());
            Log = Observable.Defer(() => _log.AsObservable().Publish().RefCount());
        }

        public bool IsConnected { get; private set; }
        public IObservable<bool> ConnectionState { get; }
        public IObservable<byte[]> Received { get; }
        public IObservable<string> Log { get; }

        public void Start(SerialPortSetting setting)
        {
            _log.OnNext("Start");
            Stop();

            IsConnected = true;
        } 

        public void Stop()
        {
            _log.OnNext("Stop");
            IsConnected = false;
        }

        public async Task SendAsync(string hex)
        {
            _log.OnNext($"Send: {hex}");
            _sendIndex++;
            if (_lostInterval > 0 && _sendIndex % _lostInterval == 0) //模拟失败
                return;
            await Task.Delay(_sendDelay);

            _received.OnNext(hex.HexToByte());
        }

        public async Task<string> SendAsync(string hex, int timeoutMs, string template, char sep = '$')
        {
            _log.OnNext($"Send: {hex}");
            _sendIndex++;
            if (_lostInterval > 0 && _sendIndex % _lostInterval == 0) //模拟失败
                return string.Empty;
            await Task.Delay(_sendDelay);

            var resp = template.TryGetTemplateMatchResult(hex.Replace(" ", ""));

            await Task.Delay(timeoutMs / 2);

            return resp;
        }

        public async Task<byte[]> SendAsync(string hex, int waitMs)
        {
            _log.OnNext($"Send: {hex}");
            _sendIndex++;
            if (_lostInterval > 0 && _sendIndex % _lostInterval == 0) //模拟失败
                return Array.Empty<byte>();
            await Task.Delay(_sendDelay);

            await Task.Delay(waitMs);

            _received.OnNext(hex.HexToByte());

            return hex.HexToByte();
        }

        public void Dispose()
        {
            _initDisposables?.Dispose();
            _connectionState?.Dispose();
            _received?.Dispose();
            _log?.Dispose();

            Stop();
        }
    }
}