using Limxc.Tools.Extensions;
using System;
using System.Reactive.Linq;
using System.Windows.Forms;

namespace DeviceTester
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
                .ObserveOn(WindowsFormsSynchronizationContext.Current)
                .Subscribe(p => button1.Enabled = p,
                ex =>
                {

                });

            connector.Datas
                .Debug("Datas")
                .ObserveOn(WindowsFormsSynchronizationContext.Current)
                .Subscribe(p =>
                {
                    richTextBox1.Clear();
                    richTextBox1.AppendText(string.Join(",", p.value));
                    richTextBox1.AppendText($"channel: {p.channel} @ {DateTime.Now:mm:ss ffff}" + Environment.NewLine);
                    richTextBox1.ScrollToCaret();
                }, ex =>
                {

                });
        }

        private void button1_Click(object sender, EventArgs e)
        {
            connector.SetHwGainValue(100);
            connector.SetHwSampleRate(5);
            connector?.Start();
        }
    }
}