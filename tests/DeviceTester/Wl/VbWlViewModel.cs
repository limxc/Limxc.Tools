using Limxc.Tools.Common;
using Limxc.Tools.DeviceComm.Protocol;
using Limxc.Tools.Extensions;
using Limxc.Tools.Extensions.DevComm;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DeviceTester.Wl
{
    public class VbWlViewModel : ReactiveObject
    {
        private IProtocol _protocol;
        private CancellationTokenSource _cts;

        public VbWlViewModel()
        {
            _protocol = new SerialPortProtocol();

            var csMsg = _protocol.ConnectionState.Select(s => s ? $"连接成功" : $"断开");
            var revMsg = _protocol.Received.Select(b => $"{b.ToHexStr().HexToAscII()}({b.ToHexStr()})");
            csMsg.Merge(revMsg)
                .Select(m => $"@{DateTime.Now:mm:ss fff} : {m}")
                .Bucket(20)
                .ToPropertyEx(this, vm => vm.Messages);

            var canStart = _protocol.ConnectionState.ObserveOn(RxApp.MainThreadScheduler);
            Start = ReactiveCommand.CreateFromTask<TaskQueue<bool>, Unit>(async task =>
            {
                _cts?.Dispose();
                _cts = new CancellationTokenSource();
                await task.Exec(_cts.Token);
                return Unit.Default;
            }, canStart
            );

            Stop = ReactiveCommand.Create<Unit, Unit>(_ =>
            {
                _cts?.Cancel();
                return Unit.Default;
            });

            this.WhenAnyValue(vm => vm.SelectedPort)
                .Throttle(TimeSpan.FromSeconds(0.5))
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .Debug()
                .SubscribeOn(TaskPoolScheduler.Default)
                .Select(async p =>
                {
                    await Open(p);
                })
                .Subscribe();

            Commands = new List<WlTask>()
            {
                new WlTask(){ Name = "测试电机",Tasks = WlCompCmds.测试电机(Send) },
                new WlTask(){ Name = "测量",Tasks = WlCompCmds.测量(Send) },
                new WlTask(){ Name = "镀汞",Tasks = WlCompCmds.镀汞(Send) },
            };
        }

        [Reactive] public IEnumerable<WlTask> Commands { get; set; }
        public IEnumerable<string> Messages { [ObservableAsProperty] get; }

        public ReactiveCommand<TaskQueue<bool>, Unit> Start { get; set; }
        public ReactiveCommand<Unit, Unit> Stop { get; set; }

        [Reactive] public string SelectedPort { get; set; }

        private async Task Open(string portName)
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

        private Task<bool> Send(string cmd)
        {
            return _protocol.SendAsync(cmd.AscIIToHex().ToByte());
        }
    }
}