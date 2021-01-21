using Limxc.Tools.DeviceComm.Protocol;
using Limxc.Tools.Extensions;
using Limxc.Tools.Extensions.DevComm;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DeviceTester.Wl
{
    public class VbWlMixViewModel : ReactiveObject
    {
        private IProtocol _protocol;

        public VbWlMixViewModel()
        {
            _protocol = new SerialPortProtocol();

            var csMsg = _protocol.ConnectionState.Select(s => s ? $"连接成功" : $"断开");
            var revMsg = _protocol.Received.Select(b => $"{b.ToHexStr().HexToAscII()}({b.ToHexStr()})");
            csMsg.Merge(revMsg)
                .Select(m => $"@{DateTime.Now:mm:ss fff} : {m}")
                .Bucket(20)
                .ToPropertyEx(this, vm => vm.Messages);

            var canSend = _protocol.ConnectionState.ObserveOn(RxApp.MainThreadScheduler);
            Send = ReactiveCommand.CreateFromTask<string, Unit>(async cmd =>
            {
                await _protocol.SendAsync(cmd.AscIIToHex().ToByte());
                return Unit.Default;
            }, canSend
            ); 
        }

        public IEnumerable<string> Messages { [ObservableAsProperty] get; }

        public ReactiveCommand<string, Unit> Send { get; set; }
         

        public async Task Open(string portName)
        {
            ((SerialPortProtocol)_protocol).Init(portName, 38400);

            try
            {
                if (!await _protocol.OpenAsync())
                {
                    throw new Exception("打开串口失败");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}