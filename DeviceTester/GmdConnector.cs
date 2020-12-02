using Limxc.Tools.DeviceComm.Entities;
using Limxc.Tools.DeviceComm.Protocol;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace DeviceTester
{
    public class GmdConnector : ReactiveObject
    {
        private int _sendPort = 9000;
        private int _receivePort = 9080;

        private string _serverIp = "192.168.0.103";
        private string _clientIp = "192.168.0.200";

        private TcpServerProtocol_SST _sendServer, _receiveServer;
        public extern string ConnectionPort { [ObservableAsProperty]get; }
        public extern bool ConnectedClients { [ObservableAsProperty]get; }

        public GmdConnector()
        {
            _sendServer = new TcpServerProtocol_SST(_serverIp, _sendPort);
            _receiveServer = new TcpServerProtocol_SST(_serverIp, _receivePort);

            _sendServer.ConnectionState
                .Select(p => p.IsConnected ? p.IpPort : string.Empty)
                .ToPropertyEx(this, x => x.ConnectionPort);

            _receiveServer.ConnectionState
                .CombineLatest(_sendServer.ConnectionState)
                .Select(r =>
                {
                    if (r.First.IpPort == r.Second.IpPort)
                        if (r.First.IsConnected && r.Second.IsConnected)
                            return true;
                    return false;
                })
                .StartWith(false)
                .ToPropertyEx(this, x => x.ConnectedClients);

            var packHead = new byte[] { 0xeb, 0x90 };
            var packTail = new byte[] { };

            //_receiveServer.Received
            //    .SelectMany(p=>p)
            //    .Buffer(2,2)
            //    .BufferUntil(new byte[] { 0xeb ,0x90},)
        }

        #region 硬件参数设置

        public async Task Send(string command, string desc = "")
        {
            var rst = await _sendServer.SendAsync(new CPContext(command, "") { ClientId = ConnectionPort });
            if (false)
                throw new System.Exception("指令发送失败.");
        }

        /// <summary>
        /// 开始
        /// </summary>
        public void Start() => Send("eb 90 00 03 01 10 000000000000000000000000000000000000000000000000ab37", "开始");

        /// <summary>
        /// 停止
        /// </summary>
        public void Stop() => Send("eb 90 00 03 01 11 0000000000000000000000000000000000000000000000009c34", "停止");

        /// <summary>
        /// 自动校准
        /// </summary>
        public void Adjust() => Send("eb 90 00 03 01 15 0000000000000000000000000000000000000000000000004038", "自动校准");

        /// <summary>
        /// 每秒采样次数 (n*4条)
        /// </summary>
        /// <param name="rate">5-25</param>
        public void SetHwSampleRate(int rate = 5)
        {
            switch (rate)
            {
                case 5:
                default:
                    Send("eb 90 00 03 01 12 0005 000000000000000000000000000000000000000000000fa8", "采样率 5/秒");
                    break;

                case 10:
                    Send("eb9000030112000a000000000000000000000000000000000000000000004022", "采样率 10/秒");
                    break;

                case 15:
                    Send("eb9000030112000f000000000000000000000000000000000000000000008abb", "采样率 15/秒");
                    break;

                case 20:
                    Send("eb9000030112001400000000000000000000000000000000000000000000df36", "采样率 20/秒");
                    break;

                case 25:
                    Send("eb90000301120019000000000000000000000000000000000000000000004b0c", "采样率 25/秒");
                    break;
            }
        }

        /// <summary>
        /// 硬件增益值
        /// </summary>
        /// <param name="value"></param>
        public void SetHwGainValue(int value)
        {
            switch (value)
            {
                case 100:
                default:
                    Send("eb 90 00 03 01 14 64 00000000000000000000000000000000000000000000005202", "增益值 100");
                    break;

                case 80:
                    Send("eb90000301145000000000000000000000000000000000000000000000008695", "增益值 80");
                    break;

                case 90:
                    Send("eb90000301145a000000000000000000000000000000000000000000000054b8", "增益值 90");
                    break;

                case 110:
                    Send("eb90000301146e0000000000000000000000000000000000000000000000802f", "增益值 110");
                    break;

                case 120:
                    Send("eb9000030114780000000000000000000000000000000000000000000000fe42", "增益值 120");
                    break;
            }
        }

        #endregion 硬件参数设置
    }
}