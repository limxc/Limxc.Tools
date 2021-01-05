using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Collections.Generic;
using System.IO.Ports;
using System.Reactive;

namespace DeviceTester.Tcf
{
    public class FrmTcfViewModel : ReactiveObject
    {
        public FrmTcfViewModel()
        {
            GetPortNames();
            GetPorts = ReactiveCommand.Create(GetPortNames);
        }

        [Reactive] public IEnumerable<string> PortNames { get; set; }

        public ReactiveCommand<Unit, Unit> GetPorts { get; }

        public void GetPortNames() => PortNames = SerialPort.GetPortNames();
    }
}