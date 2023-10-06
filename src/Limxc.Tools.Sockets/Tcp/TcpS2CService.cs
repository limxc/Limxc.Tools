using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks;
using Limxc.Tools.Extensions;
using Limxc.Tools.Extensions.Communication;

namespace Limxc.Tools.Sockets.Tcp
{
    public class TcpS2CService : ITcpS2CService
    {
        private readonly int _bufferSize = 1024;

        private readonly Subject<bool> _connectionState = new Subject<bool>();
        private readonly CompositeDisposable _initDisposables = new CompositeDisposable();
        private readonly Subject<string> _log = new Subject<string>();
        private readonly Subject<byte[]> _received = new Subject<byte[]>();
        private CancellationTokenSource _cts;

        private Socket _server, _client;

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

        public bool IsConnected { get; private set; }

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
        }

        public void Start(TcpS2CSetting setting)
        {
            _tcpS2CSetting = setting;

            Stop();

            if (_tcpS2CSetting == default)
                return;

            if (!_tcpS2CSetting.Enabled)
                return;

            var ipEndPoint = ParseIpPort(_tcpS2CSetting.ServerIpPort);
            _server = new Socket(ipEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            _server.Bind(ipEndPoint);
            _server.Listen(1);

            _cts = new CancellationTokenSource();

            Task.Run(async () =>
            {
                while (true)
                    try
                    {
                        if (_cts.Token.IsCancellationRequested)
                            return;

                        _client = await _server.AcceptAsync();
                        if (!_client.RemoteEndPoint.ToString().Contains(_tcpS2CSetting.ClientIp))
                        {
                            IsConnected = false;
                            _client.Dispose();
                            continue;
                        }

                        IsConnected = true;

                        while (true)
                        {
                            if (_cts.Token.IsCancellationRequested)
                                return;

                            var buffer = new ArraySegment<byte>(new byte[_bufferSize]);
                            var received = await _client.ReceiveAsync(buffer, SocketFlags.None);

                            _received.OnNext(buffer.Take(received).ToArray());
                        }
                    }
                    catch (Exception ex)
                    {
                        _log.OnNext($"Server Error: {ex.Message}");
                        _client?.Dispose();
                        IsConnected = false;
                    }
            }, _cts.Token);
        }

        public void Stop()
        {
            _cts?.Cancel();
            _client?.Dispose();
            _server?.Dispose();
        }

        /// <summary>
        ///     无返回值发送
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public async Task SendAsync(byte[] bytes)
        {
            try
            {
                await _client.SendAsync(new ArraySegment<byte>(bytes), SocketFlags.None);
            }
            catch (Exception ex)
            {
                _log.OnNext($"Send Error: {ex.Message}");
                throw;
            }
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

                await _client.SendAsync(new ArraySegment<byte>(hex.HexToByte()), SocketFlags.None);

                return await task;
            }
            catch (Exception ex)
            {
                _log.OnNext($"Send Error: {ex.Message}");
                throw;
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

                await _client.SendAsync(new ArraySegment<byte>(bytes), SocketFlags.None);

                return await task;
            }
            catch (Exception ex)
            {
                _log.OnNext($"Send Error: {ex.Message}");
                throw;
            }
        }

        private IPEndPoint ParseIpPort(string ipPort)
        {
            if (string.IsNullOrEmpty(ipPort)) throw new ArgumentNullException(nameof(ipPort));

            IPAddress ip = null;
            var port = -1;

            var colonIndex = ipPort.LastIndexOf(':');
            if (colonIndex != -1)
            {
                ip = IPAddress.Parse(ipPort.Substring(0, colonIndex));
                port = Convert.ToInt32(ipPort.Substring(colonIndex + 1));
            }

            return new IPEndPoint(ip ?? throw new ArgumentNullException(nameof(ipPort)), port);
        }
    }
}