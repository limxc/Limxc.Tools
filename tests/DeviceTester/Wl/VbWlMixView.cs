using GodSharp.SerialPort;
using Limxc.Tools.Extensions;
using ReactiveUI;
using System;
using System.IO.Ports;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DeviceTester.Wl
{
    public partial class VbWlMixView : Form, IViewFor<VbWlMixViewModel>
    {
        public VbWlMixView()
        {
            InitializeComponent();

            this.WhenActivated(d =>
            {
                var selectCmd = this.WhenAnyValue(v => v.dCmd.SelectedValue)
                                    .Where(p => p != null);

                this.BindCommand(ViewModel, vm => vm.Send, v => v.btnSend, selectCmd).DisposeWith(d);
            });
        }

        public VbWlMixViewModel ViewModel { get; set; }
        object IViewFor.ViewModel { get => ViewModel; set => ViewModel = (VbWlMixViewModel)value; }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            var last = dPorts.SelectedValue?.ToString();
            var ports = SerialPort.GetPortNames();
            dPorts.DataSource = ports;
            if (!string.IsNullOrWhiteSpace(last) && ports.Contains(last))
                dPorts.SelectedItem = last;
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
        }

        private void VbWlMixView_Load(object sender, EventArgs e)
        {
            //dCmd.ValueMember = nameof(WlTask.Command);
            //dCmd.DisplayMember = nameof(WlTask.Name);
            //dCmd.DataSource = WlCommands.Tasks;
        }

        private async void dPorts_SelectedValueChanged(object sender, EventArgs e)
        {
            var port = dPorts.SelectedValue.ToString();
            if (!string.IsNullOrWhiteSpace(port))
                await ViewModel.Open(port);
        }
         
    }
}