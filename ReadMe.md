
### Limxc.Tools.DeviceComm
##### 1.SerialPort 


    IProtocol sp = new SerialPortProtocol();

    sp.Connect(SerialPort.GetPortNames()[0], 9600);

    Observable.Merge
        (
            sp.History.Select(p => $"{DateTime.Now:mm:ss ffff} {p}"),
            sp.IsConnected.Select(p => $"{DateTime.Now:mm:ss ffff} State: {p}"),
            sp.Received.Select(p => $"{DateTime.Now:mm:ss ffff} receive : {p}")
        )
        .Subscribe(p =>
        {
            Console.WriteLine(p);
        });

    Observable.Interval(TimeSpan.FromSeconds(3))
        .Subscribe(async _ =>
        {
            await sp.Send(new Limxc.Tools.DeviceComm.Entities.CPContext("AA00 0a10 afBB", "AA00$2$1BB", 256));
        });
