using Limxc.Tools.CrcCSharp;
using Limxc.Tools.DeviceComm.Extensions;
using Limxc.Tools.DeviceComm.Protocol;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Windows.Forms;

namespace DeviceTester.Tcf
{
    public partial class FrmTcf : Form
    {
        public FrmTcf()
        {
            InitializeComponent();
        }

        private IProtocol _ptc;
        private Crc _crc;
        private TcfCRC _tcfCRC;
        private CompositeDisposable _disposables;

        private async void FrmTcf_Load(object sender, EventArgs e)
        {
            _disposables = new CompositeDisposable();

            _crc = new Crc(CrcStdParams.StandartParameters[CrcAlgorithms.Crc16Modbus]);
            _tcfCRC = new TcfCRC();

            InitCmd();

            _ptc = new SerialPortProtocol(SerialPort.GetPortNames().Last(), 115200);

            _ptc.Received
                .SubscribeOn(NewThreadScheduler.Default)
                .ObserveOn(SynchronizationContext.Current)
                .Subscribe(p =>
                {
                    var recv = p.ToHexStr().HexToAscII();
                    richTextBox1.AppendText($"{DateTime.Now:HH:mm:ss fff}{Environment.NewLine}{JsonConvert.SerializeObject(recv)}{Environment.NewLine}");
                    richTextBox1.ScrollToCaret();
                })
                .DisposeWith(_disposables);

            _ptc.ConnectionState
                .SubscribeOn(NewThreadScheduler.Default)
                .ObserveOn(SynchronizationContext.Current)
                .Subscribe(isConnected =>
                {
                    richTextBox1.AppendText($"{DateTime.Now:HH:mm:ss fff} {(isConnected ? "连接成功" : "连接失败")} \r\n");
                    richTextBox1.ScrollToCaret();
                })
                .DisposeWith(_disposables);

            var enableAutoConnect = Observable
                .FromEventPattern(h => cbAutoConnect.CheckedChanged += h, h => cbAutoConnect.CheckedChanged -= h)
                .Select(_ => cbAutoConnect.Checked)
                .StartWith(cbAutoConnect.Checked)
                .Do(c => Debug.WriteLine($"EnableAutoConnect :{c}"))
                ;

            var autoReconnect = Observable.Interval(TimeSpan.FromSeconds(1))
                .CombineLatest(_ptc.ConnectionState)
                .Select(p => p.Second)
                //.Do(c => Debug.WriteLine($"IsConnected :{c}"))
                .Where(p => !p)
                .Do(async _ =>
                {
                    var s = await _ptc.OpenAsync();
                    //Debug.WriteLine($"Open Port: {s}");
                });

            enableAutoConnect
                .Select(enabled => enabled ? autoReconnect : Observable.Empty<bool>())
                .Switch()
                .Retry()
                .Subscribe()
                .DisposeWith(_disposables);

            var r = await _ptc.OpenAsync();
        }

        private void InitCmd()
        {
            listBox1.Items.Add(TcfCommands.QueryId());
            listBox1.Items.Add(TcfCommands.QueryStep());
            listBox1.Items.Add(TcfCommands.QueryWeight());
            listBox1.Items.Add(TcfCommands.SendId("123456"));
            listBox1.Items.Add(TcfCommands.SendId("111111"));
            listBox1.Items.Add(TcfCommands.SendHeight(155.5));
            listBox1.Items.Add(TcfCommands.SendHeight(115.5));
            listBox1.Items.Add(TcfCommands.SendAge(24));
            listBox1.Items.Add(TcfCommands.SendAge(5));
            listBox1.Items.Add(TcfCommands.SendGender("男"));
            listBox1.Items.Add(TcfCommands.SendGender("女"));
            listBox1.Items.Add(TcfCommands.Measure());
            listBox1.Items.Add(TcfCommands.QueryLastError());
            listBox1.Items.Add(TcfCommands.Return());
            listBox1.Items.Add(TcfCommands.ReturnHome());
            listBox1.Items.Add(TcfCommands.Read());

            listBox2.Items.Add(nameof(TcfCommands.QueryId));
            listBox2.Items.Add(nameof(TcfCommands.QueryStep));
            listBox2.Items.Add(nameof(TcfCommands.QueryWeight));
            listBox2.Items.Add(nameof(TcfCommands.SendId));
            listBox2.Items.Add(nameof(TcfCommands.SendHeight));
            listBox2.Items.Add(nameof(TcfCommands.SendAge));
            listBox2.Items.Add(nameof(TcfCommands.SendGender));
            listBox2.Items.Add(nameof(TcfCommands.Measure));
            listBox2.Items.Add(nameof(TcfCommands.QueryLastError));
            listBox2.Items.Add(nameof(TcfCommands.Return));
            listBox2.Items.Add(nameof(TcfCommands.ReturnHome));
            listBox2.Items.Add(nameof(TcfCommands.Read));
        }

        private void listBox1_SelectedValueChanged(object sender, EventArgs e)
        {
            textBox1.Text = listBox1.SelectedItem?.ToString();
        }

        private byte[] GetCmd(string cmd)
        {
            //厂家
            var c = _tcfCRC.CRCEfficacy(cmd);
            var b = (c + cmd).AscIIToHex().ToByte();

            Debug.WriteLine($"{c}_{cmd} {b.ToHexStr()}");

            return b;
        }

        private void FrmTcf_FormClosing(object sender, FormClosingEventArgs e)
        {
            _disposables?.Dispose();
            _ptc?.CleanUp();
        }

        private async void btnSend_Click(object sender, EventArgs e)
        {
            var cmd = GetCmd(textBox1.Text);
            await _ptc.SendAsync(cmd);
        }
    }
}