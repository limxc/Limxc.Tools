using Limxc.Tools.DeviceComm.Protocol;
using ReactiveUI;
using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows.Forms;

namespace DeviceTester.Tcf
{
    public partial class FrmTcfView : Form, IViewFor<FrmTcfViewModel>
    {
        public FrmTcfView()
        {
            InitializeComponent();

            this.StartPosition = FormStartPosition.CenterScreen;

            this.WhenActivated(d =>
            {
                this.dSerialPort.Events().SelectedValueChanged
                    .Subscribe(_ =>
                    {
                        CleanUp();

                        var portName = dSerialPort.SelectedItem.ToString();
                        var ipc = new SerialPortProtocol(portName, 115200);
                        viewModelControlHost1.ViewModel = new TcfViewModel(ipc); ;
                    })
                    .DisposeWith(d);

                this.OneWayBind(ViewModel, vm => vm.PortNames, v => v.dSerialPort.DataSource).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.GetPorts, v => v.btnRefresh).DisposeWith(d);

                this.Events().FormClosing.Subscribe(_ => CleanUp()).DisposeWith(d);
            });
        }

        public FrmTcfViewModel ViewModel { get; set; }
        object IViewFor.ViewModel { get => ViewModel; set => ViewModel = (FrmTcfViewModel)value; }

        private void CleanUp()
        {
            if (viewModelControlHost1.ViewModel != null)
            {
                ((TcfViewModel)viewModelControlHost1.ViewModel).CleanUp();
            }
        }
    }
}