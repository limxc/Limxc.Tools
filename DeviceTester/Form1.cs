using Limxc.Tools.DeviceComm.Entities;
using Limxc.Tools.DeviceComm.Protocol;
using System;
using System.Reactive.Linq;
using System.Windows.Forms;

namespace DeviceTester
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private SerialPortProtocol_GodSharp spp;

        private void Form1_Load(object sender, EventArgs e)
        {
            spp = new SerialPortProtocol_GodSharp();
            spp.History.Subscribe(p => Log($"--- history : {p.ToString()}"));
            spp.IsConnected.Subscribe(p => Log($"--- connected : {p.ToString()}"));
            spp.Received.Subscribe(p => Log($"--- received : {p.ToString()}"));

            spp.Connect("COM12", 9600);

        }

        private void Log(string msg)
        {
            richTextBox1.AppendText(msg + Environment.NewLine);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            spp.Send(new CPContext("AA0102af0304BB", "AA0102$10304BB", 20));
        }
    }
}