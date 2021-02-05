using Limxc.Tools.Extensions;
using ReactiveUI;
using System;
using System.IO.Ports;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows.Forms;

namespace DeviceTester.Wl
{
    public partial class VbWlView : Form, IViewFor<VbWlViewModel>
    {
        public VbWlView()
        {
            InitializeComponent();

            dCmd.ValueMember = nameof(WlTask.Tasks);
            dCmd.DisplayMember = nameof(WlTask.Name);

            this.WhenActivated(d =>
            {
                this.OneWayBind(ViewModel, vm => vm.Commands, v => v.dCmd.DataSource).DisposeWith(d);

                var selectCmd = this.WhenAnyValue(v => v.dCmd.SelectedValue)
                                    .Where(p => p != null)
                                    .Do(p=> { 
                                    
                                    });

                this.BindCommand(ViewModel, vm => vm.Start, v => v.btnStart, selectCmd).DisposeWith(d); 
                this.BindCommand(ViewModel, vm => vm.Stop, v => v.btnStop).DisposeWith(d);

                ViewModel.WhenAnyValue(vm => vm.Messages)
                     .ObserveOn(RxApp.MainThreadScheduler)
                     .Subscribe(m => dMsg.Lines = m?.ToArray())
                     .DisposeWith(d);

                this.btnRefresh.Events().Click
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(_ =>
                    {
                        var last = dPorts.SelectedValue?.ToString();
                        var ports = SerialPort.GetPortNames();
                        dPorts.DataSource = ports;
                        if (!string.IsNullOrWhiteSpace(last) && ports.Contains(last))
                            dPorts.SelectedItem = last;
                    })
                    .DisposeWith(d); 

                this.Bind(ViewModel, vm => vm.SelectedPort, v => v.dPorts.SelectedValue, vmc => vmc, vc => vc?.ToString()).DisposeWith(d);
            });
        }

        public VbWlViewModel ViewModel { get; set; }
        object IViewFor.ViewModel { get => ViewModel; set => ViewModel = (VbWlViewModel)value; }
    }
}