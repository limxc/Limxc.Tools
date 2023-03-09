using System;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using Limxc.Tools.Extensions;
using Limxc.Tools.Extensions.Communication;
using SuperSimpleTcp;

namespace Limxc.Tools.Sockets.Tcp
{
    public class TcpS2CService : ITcpS2CService
    {
        private readonly Subject<bool> _connectionState = new Subject<bool>();
        private readonly CompositeDisposable _initDisposables = new CompositeDisposable();
        private readonly Subject<string> _log = new Subject<string>();
        private readonly Subject<byte[]> _received = new Subject<byte[]>();
        private string _clientIpPort;

        private CompositeDisposable _controlDisposables = new CompositeDisposable();
        private SimpleTcpServer _server;
        private TcpS2CSetting _tcpS2CSetting;

        public TcpS2CService()
        {
            Observable
                .Interval(TimeSpan.FromSeconds(1))
                .Select(_ => IsConnected)
                .SubscribeOn(TaskPoolScheduler.Default)
                .Subscribe(s => _connectionState.OnNext(s))
                .DisposeWith(_initDisposables);

            ConnectionState = Observable.Defer(() =>
                _connectionState.StartWith(false).AsObservable().Publish().RefCount());
            Received = Observable.Defer(() =>
                _received.AsObservable().Publish().RefCount());
            Log = Observable.Defer(() => _log.AsObservable().Publish().RefCount());
        }

        public bool IsConnected =>
            _server?.GetClients().Any(p => p.StartsWith(_tcpS2CSetting?.ClientIp ?? "---")) ?? false;

        public IObservable<bool> ConnectionState { get; }

        /// <summary>
        ///     .SubscribeOn(new EventLoopScheduler())
        /// </summary>
        public IObservable<byte[]> Received { get; }

        public IObservable<string> Log { get; }

        public void Dispose()
        {
            _initDisposables?.Dispose();
            _connectionState?.Dispose();
            _received?.Dispose();
            _log?.Dispose();

            Stop();
            _server.Dispose();
        }

        public void Start(TcpS2CSetting setting)
        {
            _tcpS2CSetting = setting;

            Stop();

            if (_tcpS2CSetting == default)
                return;

            if (!_tcpS2CSetting.Enabled)
                return;

            _controlDisposables = new CompositeDisposable();

            _server = new SimpleTcpServer(_tcpS2CSetting.ServerIpPort);
            _server.Keepalive.EnableTcpKeepAlives = true;
            _server.Keepalive.TcpKeepAliveInterval = 5; // seconds to wait before sending subsequent keepalive
            _server.Keepalive.TcpKeepAliveTime = 5; // seconds to wait before sending a keepalive
            _server.Keepalive.TcpKeepAliveRetryCount =
                5; // number of failed keepalive probes before terminating connection

            Observable.FromEventPattern<ConnectionEventArgs>(_server.Events, nameof(_server.Events.ClientConnected))
                .Where(p => p.EventArgs.IpPort.StartsWith(_tcpS2CSetting?.ClientIp ?? "---"))
                .Subscribe(s =>
                {
                    _connectionState.OnNext(true);
                    _clientIpPort = s.EventArgs.IpPort;
                })
                .DisposeWith(_initDisposables);

            Observable.FromEventPattern<ConnectionEventArgs>(_server.Events, nameof(_server.Events.ClientDisconnected))
                .Where(p => p.EventArgs.IpPort.StartsWith(_tcpS2CSetting?.ClientIp ?? "---"))
                .Subscribe(s =>
                {
                    _connectionState.OnNext(false);
                    _clientIpPort = string.Empty;
                })
                .DisposeWith(_initDisposables);

            Observable
                .FromEventPattern<DataReceivedEventArgs>(_server.Events, nameof(_server.Events.DataReceived))
                .SubscribeOn(new EventLoopScheduler())
                .Subscribe(b => _received.OnNext(b.EventArgs.Data.ToArray()))
                .DisposeWith(_controlDisposables);

            _server.Start();
        }

        public void Stop()
        {
            _controlDisposables?.Dispose();

            _server?.Stop();
        }

        /// <summary>
        ///     无返回值发送
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public async Task SendAsync(byte[] bytes)
        {
            await _server.SendAsync(_clientIpPort, bytes);
        }

        /// <summary>
        ///     在超时时长内,根据模板自动截取返回值
        /// </summary>
        /// <param name="hex"></param>
        /// <param name="timeoutMs">ms</param>
        /// <param name="template"></param>
        /// <param name="sepBegin"></param>
        /// <param name="sepEnd"></param>
        /// <returns></returns>
        public async Task<string> SendAsync(string hex, int timeoutMs, string template, char sepBegin = '[',
            char sepEnd = ']')
        {
            try
            {
                var now = DateTimeOffset.Now;
                var task = _received
                    .SkipUntil(now)
                    .TakeUntil(now.AddMilliseconds(timeoutMs))
                    .Select(d => d.ByteToHex())
                    .Scan((acc, r) => acc + r)
                    .Select(r => r.TryGetTemplateMatchResults(template, sepBegin, sepEnd).FirstOrDefault())
                    .ToTask();

                await _server.SendAsync(_clientIpPort, hex.HexToByte());

                return await task;
            }
            catch (Exception ex)
            {
                _log.OnNext($"Send Error: {ex.Message}");
                return string.Empty;
            }
        }

        /// <summary>
        ///     发送后等待固定时长后获取返回值, 需后续手动截取
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="waitMs"></param>
        /// <returns></returns>
        public async Task<byte[]> SendAsync(byte[] bytes, int waitMs)
        {
            try
            {
                var now = DateTimeOffset.Now;
                var task = _received.SkipUntil(now).TakeUntil(now.AddMilliseconds(waitMs))
                    .Aggregate((x, y) => x.Concat(y).ToArray()).ToTask();

                await _server.SendAsync(_clientIpPort, bytes);

                return await task;
            }
            catch (Exception ex)
            {
                _log.OnNext($"Send Error: {ex.Message}");
                return Array.Empty<byte>();
            }
        }
    }
}