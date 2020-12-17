using Limxc.Tools.DeviceComm.Entities;
using Limxc.Tools.DeviceComm.Extensions;
using Limxc.Tools.DeviceComm.Protocol;
using Limxc.Tools.Extensions;
using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace DeviceTester.Gmd
{
    public class GmdConnector
    {
        private int _sendPort = 9000;
        private int _receivePort = 9080;

        private readonly string _serverIp = "192.168.0.103";
        private readonly string _clientIp = "192.168.0.200";

        private TcpServerProtocol_SST _sendServer, _receiveServer;

        public IObservable<bool> IsConnected { get; }
        public IObservable<(int channel, int[] value)> Datas { get; }

        public GmdConnector()
        {
            _sendServer = new TcpServerProtocol_SST(_serverIp + ":" + _sendPort, _clientIp + ":" + _sendPort);
            _receiveServer = new TcpServerProtocol_SST(_serverIp + ":" + _receivePort, "");

            IsConnected = _sendServer.ConnectionState
                .CombineLatest(_receiveServer.ConnectionState)
                .Select(p => p.First && p.Second);

            Datas = _receiveServer
                .Received
                .SelectMany(p => p)
                .ParsePackage(packHead)
                .Where(p => p.Length == 4096 - 2)
                .Select(pack =>
                {
                    var channel = pack.Take(2).ToArray().ToInt();
                    //2046个点
                    var value = pack.Skip(2).Split(2).Select(p => p.ToArray().ToInt()).ToArray();
                    return (channel, value);
                })
                .Retry()
                .Publish()
                .RefCount();
        }

        private readonly byte[] packHead = new byte[] { 0xeb, 0x90 };

        public void Open()
        {
            _sendServer.OpenAsync();
            _receiveServer.OpenAsync();
        }

        public void Close()
        {
            _sendServer.CloseAsync();
            _receiveServer.CloseAsync();
        }

        public void CleanUp()
        {
            _sendServer?.CleanUp();
            _receiveServer?.CleanUp();
        }

        #region 硬件参数设置

        public async Task<bool> Send(string command, string desc = "")
        {
            var r = await _sendServer.SendAsync(new CPContext(command, desc));
            return r;
        }

        /// <summary>
        /// 开始
        /// </summary>
        public Task<bool> Start() => Send("eb 90 00 03 01 10 000000000000000000000000000000000000000000000000ab37", "开始");

        /// <summary>
        /// 停止
        /// </summary>
        public Task<bool> Stop() => Send("eb 90 00 03 01 11 0000000000000000000000000000000000000000000000009c34", "停止");

        /// <summary>
        /// 自动校准
        /// </summary>
        public Task<bool> Adjust() => Send("eb 90 00 03 01 15 0000000000000000000000000000000000000000000000004038", "自动校准");

        /// <summary>
        /// 每秒采样次数 (n*4条)
        /// </summary>
        /// <param name="rate">5,10,15,20,25</param>
        public Task<bool> SetHwSampleRate(int rate = 5)
        {
            switch (rate)
            {
                case 5:
                default:
                    return Send("eb 90 00 03 01 12 0005 000000000000000000000000000000000000000000000fa8", "采样率 5/秒");

                case 10:
                    return Send("eb9000030112000a000000000000000000000000000000000000000000004022", "采样率 10/秒");

                case 15:
                    return Send("eb9000030112000f000000000000000000000000000000000000000000008abb", "采样率 15/秒");

                case 20:
                    return Send("eb9000030112001400000000000000000000000000000000000000000000df36", "采样率 20/秒");

                case 25:
                    return Send("eb90000301120019000000000000000000000000000000000000000000004b0c", "采样率 25/秒");
            }
        }

        /// <summary>
        /// 硬件增益值
        /// </summary>
        /// <param name="value">80,90,100,110,120</param>
        public Task<bool> SetHwGainValue(int value)
        {
            switch (value)
            {
                case 100:
                default:
                    return Send("eb 90 00 03 01 14 64 00000000000000000000000000000000000000000000005202", "增益值 100");

                case 80:
                    return Send("eb90000301145000000000000000000000000000000000000000000000008695", "增益值 80");

                case 90:
                    return Send("eb90000301145a000000000000000000000000000000000000000000000054b8", "增益值 90");

                case 110:
                    return Send("eb90000301146e0000000000000000000000000000000000000000000000802f", "增益值 110");

                case 120:
                    return Send("eb9000030114780000000000000000000000000000000000000000000000fe42", "增益值 120");
            }
        }

        #endregion 硬件参数设置
    }
}