using Limxc.Tools.DeviceComm.Extensions;
using Limxc.Tools.DeviceComm.Protocol;
using System;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
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

        private async void button1_Click(object sender, EventArgs e)
        {
            var cmd = GetCmd(textBox1.Text);
            await _ptc.SendAsync(cmd);
        }

        private IProtocol _ptc;

        private async void FrmTcf_Load(object sender, EventArgs e)
        {
            InitCmd();

            _ptc = new SerialPortProtocol(SerialPort.GetPortNames().Last(), 115200);

            _ptc.Received
                .SubscribeOn(SynchronizationContext.Current)
                .Subscribe(p =>
                {
                    var recv = p.Skip(4).ToArray().ToHexStr().HexToAscII();
                    richTextBox1.AppendText($"{DateTime.Now:HH:mm:ss fff} {recv} \r\n");
                });

            _ptc.ConnectionState
                .SubscribeOn(SynchronizationContext.Current)
                .Select(s => s ? "连接成功" : "连接失败")
                .Subscribe(s =>
                {
                    richTextBox1.AppendText($"{DateTime.Now:HH:mm:ss fff} {s} \r\n");
                });

            var r = await _ptc.OpenAsync();
        }

        private void InitCmd()
        {
            listBox1.Items.Add("*idn?\n");
            listBox1.Items.Add("*wei?\n");
            listBox1.Items.Add("*stp?\n");

            listBox1.Items.Add("*id=123456\n");
            listBox1.Items.Add("*hei=155.4\n");

            listBox1.Items.Add("*ret\n");

            listBox1.Items.Add("*mea\n");
            listBox1.Items.Add("*read?\n");
        }

        private void listBox1_SelectedValueChanged(object sender, EventArgs e)
        {
            textBox1.Text = listBox1.SelectedItem.ToString();
        }

        private byte[] GetCmd(string cmd)
        {
            var bs = cmd.AscIIToHex().ToByte();
            var crc = bs.Crc32();
            var combine = crc.Concat(bs).ToArray();

            Debug.WriteLine($"{cmd} {bs.Crc32().ToHexStr()}-{cmd.AscIIToHex()} {combine.ToHexStr()}");

            return combine;
        }

        private void FrmTcf_FormClosing(object sender, FormClosingEventArgs e)
        {
            _ptc?.CleanUp();
        }
    }
}