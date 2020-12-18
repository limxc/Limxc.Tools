using GodSharp.SerialPort;
using Limxc.Tools.DeviceComm.Extensions;
using System;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
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

        private void button1_Click(object sender, EventArgs e)
        {
            var cmd = GetCmd(textBox1.Text);
            _sp.Write(cmd);

            Thread.Sleep(100);
            richTextBox1.AppendText(_sp.ReadString() + Environment.NewLine);
            //richTextBox1.AppendText(_sp.ReadLine() + Environment.NewLine);
            richTextBox1.AppendText(_sp.ReadExisting() + Environment.NewLine);
        }

        private GodSerialPort _sp;

        private void FrmTcf_Load(object sender, EventArgs e)
        {
            _sp = new GodSerialPort(SerialPort.GetPortNames().Last(), 115200, 0);
            _sp.ReadTimeout = 1000;

            var r = _sp.Open();

            richTextBox1.AppendText(r ? "连接成功" : "连接失败" + Environment.NewLine);

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
    }
}