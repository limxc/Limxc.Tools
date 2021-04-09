using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Limxc.Tools.Core.Abstractions;
using Limxc.Tools.Fx.Report;

namespace WinformTester
{
    public partial class Form1 : Form
    {
        private readonly ReportData rd = new ReportData();

        private readonly ReportService rs = new ReportService();

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            var rst = await rs.PrintAsync(rd, @"Reports\Default.frx", ReportMode.Design, @"output\a.pdf");
            label1.Text = DateTime.Now + "  " + rst.Length;
        }

        private async void button2_Click(object sender, EventArgs e)
        {
            var rst = await rs.PrintAsync(rd, @"Reports\Default.frx", ReportMode.Show, @"output\b.pdf");
            label1.Text = DateTime.Now + "  " + rst.Length;
        }

        private async void button3_Click(object sender, EventArgs e)
        {
            var rst = await rs.PrintAsync(rd, @"Reports\Default.frx", ReportMode.Print, @"output\c.pdf");
            label1.Text = DateTime.Now + "  " + rst.Length;
        }

        private void button4_Click(object sender, EventArgs e)
        {
        }

        public class ReportData
        {
            public string Str { get; set; } = "aaa";
            public DateTime DateTime => DateTime.Now;
            public List<int> Ints { get; set; } = new List<int> {1, 2, 3, 4};
        }
    }
}