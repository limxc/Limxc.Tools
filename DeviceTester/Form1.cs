using Limxc.Tools.DeviceComm.Entities;
using Limxc.Tools.DeviceComm.Protocol;
using ReactiveUI;
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

        private IProtocol sp;

        private void Form1_Load(object sender, EventArgs e)
        {
            sp = new SerialPortProtocol_SPS();
            sp = new SerialPortProtocol();

            sp.IsConnected
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(p => Log($"--- connected : {p.ToString()}"));
            sp.Received
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(p => Log($"--- received : {p.ToString()}"));
            sp.History
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(p => Log($"--- history : {p.ToString()}"));

            sp.Connect("COM12", 9600);
        }

        private void Log(string msg)
        {
            richTextBox1.AppendText(msg + Environment.NewLine);
        }

        private void button1_Click(object sender, EventArgs ea)
        {
            var cmd = new CPContext("AA01021a0304BB", "AA0102$10304BB", Convert.ToInt32(textBox1.Text));
            sp.Send(cmd);
            //var r = CPTool.Send("Com12",9600,cmd.ToCommand());
            //Debug.Write(r);

            string result;

            //var sp = new GodSerialPort("Com12", 9600, 0);
            //sp.UseDataReceived(true, (gs, data) =>
            //{
            //    var r = data.ToHexStr();
            //});
            ////sp.TryReadSpanTime = 20;
            //if (sp.Open())
            //{
            //    sp.WriteHexString(cmd.ToCommand());
            //    //result = sp.ReadString();
            //}
        }

        private void button2_Click(object sender, EventArgs e)
        {
            sp.Connect("COM12", 9600);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            sp.Disconnect();
        }
    }
}