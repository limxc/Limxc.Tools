using Limxc.Tools.DeviceComm.Protocol;
using Limxc.Tools.Entities.DevComm;
using ReactiveUI;
using System;
using System.IO.Ports;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Windows.Forms;

namespace DeviceTester
{
    public partial class FrmSerialPort : Form
    {
        public FrmSerialPort()
        {
            InitializeComponent();
        }

        private IProtocol sp;

        private void Form1_Load(object sender, EventArgs e)
        {
            sp = new SerialPortProtocol_SPS(SerialPort.GetPortNames()[0], 9600);
            sp = new SerialPortProtocol(SerialPort.GetPortNames()[0], 9600);

            sp.ConnectionState
                .SubscribeOn(NewThreadScheduler.Default)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(p => Log($"--- connected : {p}"));
            sp.Received
                .SubscribeOn(NewThreadScheduler.Default)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(p => Log($"--- received : {p}"));
            sp.History
                .SubscribeOn(NewThreadScheduler.Default)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(p => Log($"--- history : {p}"));

            sp.OpenAsync();
        }

        private void Log(string msg)
        {
            richTextBox1.AppendText(msg + Environment.NewLine);
        }

        private void button1_Click(object sender, EventArgs ea)
        {
            var cmd = new CPContext("AA01021a0304BB", "AA0102$10304BB", Convert.ToInt32(textBox1.Text));
            sp.SendAsync(cmd);

            //string result;
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
            sp.OpenAsync();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            sp.CloseAsync();
        }
    }
}