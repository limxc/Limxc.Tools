using System;
using System.Collections.Generic;
using System.IO;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Limxc.Tools.Extensions;
using Limxc.Tools.Utils;

namespace TestHost;

public partial class FileTesterFrm : Form
{
    private const string FilePath = "testfile.txt";

    private readonly List<Bindings> _bindings = new();

    private BindingEntity _entity = new();

    public FileTesterFrm()
    {
        InitializeComponent();

        CreateBindings();

        Observable
            .Interval(TimeSpan.FromSeconds(1))
            .Take(6)
            .Delay(TimeSpan.FromSeconds(1))
            .ObserveOn(SynchronizationContext.Current)
            .Subscribe(p =>
            {
                _entity.Message = DateTime.Now.ToLongTimeString();
                _entity.Value++;
                _entity.Time = DateTime.Now.AddDays((int)_entity.Value);
                _entity.TimeNull = DateTime.Now.AddDays((int)_entity.Value * 2);
                _entity.Inners.Add(
                    new Inner
                    {
                        Key = Random.Shared.Next().ToString(),
                        Value = Random.Shared.NextDouble().ToString()
                    }
                );
                _entity.Inner = new Inner
                {
                    Key = Random.Shared.Next().ToString(),
                    Value = Random.Shared.NextDouble().ToString()
                };
                _entity.IntValue += 10;
                //Bindings.InvalidateMember(() => _entity.Message);
            });
    }

    private void btnRefresh_Click(object sender, EventArgs e)
    {
        rtbBindingLog.Text = _entity.ToString();
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

        await using var fs = new FileStream(
            FilePath,
            FileMode.OpenOrCreate,
            FileAccess.ReadWrite,
            FileShare.ReadWrite,
            4096,
            FileOptions.Asynchronous
        );
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
        using (
            var fs = new FileStream(
                FilePath,
                FileMode.OpenOrCreate,
                FileAccess.Read,
                FileShare.ReadWrite
            )
        )
        using (var sr = new StreamReader(fs, Encoding.Default))
        {
            string line;
            while ((line = await sr.ReadLineAsync()) != null)
                rtbTxt.AppendText(line + Environment.NewLine);
        }
    }

    private void CreateBindings()
    {
        _bindings.Unbind();
        _bindings.Clear();

        Bindings.Error += obj => { obj.Debug(); };
        Bindings.Create(() => tbMessage.Text == _entity.Message).UnbindWith(_bindings);
        Bindings.Create(() => nudValue.Value == _entity.Value).UnbindWith(_bindings);
        Bindings.Create(() => dgvInner.DataSource == _entity.Inners).UnbindWith(_bindings);
        Bindings.Create(() => dtpTime.Value == _entity.Time).UnbindWith(_bindings);
        Bindings.Create(() => dtpTimeNull.Value == _entity.TimeNull).UnbindWith(_bindings);
        Bindings.Create(() => tbIntValue.Text == _entity.IntValue + "").UnbindWith(_bindings);
        Bindings.Create(() => tbInnerValue.Text == _entity.Inner.Value + "").UnbindWith(_bindings);

        Bindings
            .Create(
                () =>
                    rtbBindingLog.Text
                    == _entity.Message
                    + " | "
                    + //��Ҫһ�����Դ�������
                    _entity
            )
            .UnbindWith(_bindings);
    }

    private void button1_Click(object sender, EventArgs e)
    {
        _entity = new BindingEntity { Message = "new entity" };
        CreateBindings();
    }
}