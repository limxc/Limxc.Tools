using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Limxc.Tools.Core.Extensions;

namespace TestHost
{
    public partial class FileTesterFrm : Form
    {
        private const string FilePath = "testfile.txt";

        public FileTesterFrm()
        {
            InitializeComponent();
        }

        private void btnSaveFile_Click(object sender, EventArgs e)
        {
            $"{DateTime.Now:HH:mm:ss tt zz}".Save(FilePath, cbFileAppend.Checked);
        }

        private async void btnSaveFileAsync_Click(object sender, EventArgs e)
        {
            await $"{DateTime.Now:HH:mm:ss tt zz}".SaveAsync(FilePath, cbFileAppend.Checked);
        }

        private void btnLoadFile_Click(object sender, EventArgs e)
        {
            rtbTxt.Text = FilePath.Load();
        }

        private async void btnLoadFileAsync_Click(object sender, EventArgs e)
        {
            rtbTxt.Text = await FilePath.LoadAsync();
        }

        private async void btnWrite3s_Click(object sender, EventArgs e)
        {
            await Task.Delay(500);

            var encoding = Encoding.Default;

            await using var fs = new FileStream(FilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite,
                FileShare.ReadWrite, 4096, FileOptions.Asynchronous);
            fs.SetLength(0);
            await using var sr = new StreamWriter(fs, encoding);

            for (var i = 0; i < 10; i++)
            {
                var msg = i.ToString().ToCharArray();
                await sr.WriteLineAsync(msg);
                await sr.FlushAsync();
                await Task.Delay(300);
            }
        }

        private async void btnRead3s_Click(object sender, EventArgs e)
        {
            rtbTxt.Clear();
            using (var fs = new FileStream(FilePath, FileMode.OpenOrCreate, FileAccess.Read, FileShare.ReadWrite))
            using (var sr = new StreamReader(fs, Encoding.Default))
            {
                string line;
                while ((line = await sr.ReadLineAsync()) != null) rtbTxt.AppendText(line + Environment.NewLine);
            }
        }
    }
}