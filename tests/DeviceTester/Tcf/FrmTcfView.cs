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
                viewModelControlHost1.ViewModel = new TcfViewModel();

                this.dSerialPort.Events().SelectedValueChanged
                    .Subscribe(_ =>
                    { 
                        var portName = dSerialPort.SelectedItem.ToString();
                        var ipc = new SerialPortProtocol(portName, 115200);

                        ((TcfViewModel)viewModelControlHost1.ViewModel).SetProtocol(ipc);
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
            //if (viewModelControlHost1.ViewModel != null)
            //{
            //    var vm = ((TcfViewModel)viewModelControlHost1.ViewModel); 
            //    vm.CleanUp();
            //    viewModelControlHost1.CurrentView.Dispose();
            //}
        }
    }
}