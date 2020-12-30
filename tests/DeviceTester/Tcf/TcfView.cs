using ReactiveUI;
using ReactiveUI.Validation.Extensions;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows.Forms;

namespace DeviceTester.Tcf
{
    public partial class TcfView : Form, IViewFor<TcfViewModel>
    {
        public TcfView()
        {
            InitializeComponent();

            this.WhenActivated(d =>
            {
                ViewModel.Connect.Execute();

                ViewModel.WhenAnyValue(p => p.IsConnected)
                    .Where(p => p)
                    .Select(_ => Unit.Default)
                    .InvokeCommand(ViewModel.Reset)
                    .DisposeWith(d);

                if (Debugger.IsAttached)
                {
                    ViewModel.Id = "123456";
                    ViewModel.Name = "张三";
                    //ViewModel.Weight = 55;
                    //ViewModel.Step = 2;
                }

                this.Bind(ViewModel, vm => vm.EnableAutoReconnect, v => v.dAutoConnect.Checked).DisposeWith(d);

                this.OneWayBind(ViewModel, vm => vm.Id, v => v.dId.Text).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.Name, v => v.dName.Text).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.Height, v => v.dHeight.Value, vmp => (decimal)vmp, vp => (double)vp).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.Weight, v => v.dWeight.Value, vmp => (decimal)vmp, vp => (double)vp).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.Age, v => v.dAge.Value, vmp => (decimal)vmp, vp => (int)vp).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.Gender, v => v.dGenderMale.Checked, vmp => vmp == "男", vp => vp ? "男" : "女").DisposeWith(d);

                //this.OneWayBind(ViewModel, vm => vm.ReceivedMsg, v => v.dReceived.Lines, s => s?.ToArray()).DisposeWith(d);
                //this.OneWayBind(ViewModel, vm => vm.ResultList, v => v.dResult.DataSource).DisposeWith(d);

                ViewModel.WhenAnyValue(vm => vm.ReceivedMsg)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(m => dReceived.Lines = m?.ToArray())
                    .DisposeWith(d);
                
                ViewModel.WhenAnyValue(vm => vm.ResultList)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(m => dResult.DataSource = m)
                    .DisposeWith(d);


                this.BindCommand(ViewModel, vm => vm.Measure, v => v.btnSend).DisposeWith(d);

                this.Events().FormClosing.Select(_ => Unit.Default).InvokeCommand(ViewModel.CleanUp).DisposeWith(d);

                this.BindValidation(ViewModel, v => v.dMsg.Text).DisposeWith(d);
            });
        }

        public TcfViewModel ViewModel { get; set; }
        object IViewFor.ViewModel { get => ViewModel; set => ViewModel = (TcfViewModel)value; }
    }
}