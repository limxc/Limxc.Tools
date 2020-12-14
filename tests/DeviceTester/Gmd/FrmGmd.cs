using Limxc.Tools.Extensions;
using System;
using System.Diagnostics;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Windows.Forms;

namespace DeviceTester.Gmd
{
    public partial class FrmGmd : Form
    {
        public FrmGmd()
        {
            InitializeComponent();
        }

        private GmdConnector connector;

        private void FrmGmd_Load(object sender, EventArgs e)
        {
            connector = new GmdConnector();
            connector.Open();
            
            connector.IsConnected
                .StartWith(false)
                .Debug("IsConnected")
                .SubscribeOn(NewThreadScheduler.Default)
                .ObserveOn(WindowsFormsSynchronizationContext.Current)
                .Subscribe(p => button1.Enabled = p,
                ex =>
                {
                    Debug.WriteLine("Connected Error : " + ex.Message);
                });

            connector.Datas
                .Debug("Datas")
                .SubscribeOn(NewThreadScheduler.Default)
                .ObserveOn(WindowsFormsSynchronizationContext.Current)
                .Subscribe(p =>
                {
                    richTextBox1.Clear();
                    richTextBox1.AppendText(string.Join(",", p.value));
                    richTextBox1.AppendText($"channel: {p.channel} @ {DateTime.Now:mm:ss fff}" + Environment.NewLine);
                    richTextBox1.ScrollToCaret();
                }, ex =>
                {
                    Debug.WriteLine("Data Received Error : " + ex.Message);
                });
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            await connector.SetHwGainValue(100);
            await connector.SetHwSampleRate(5);
            await connector.Start();
        }

        private async void button2_Click(object sender, EventArgs e)
        {
            await connector.Stop();
        }

        private async void button3_Click(object sender, EventArgs e)
        {
            await connector.Adjust();
        }
    }
}