
### Limxc.Tools.DeviceComm
##### 1.SerialPort 
 
    IProtocol sp = new SerialPortProtocol();

    sp.Connect(SerialPort.GetPortNames()[0], 9600);

    Observable.Merge
        (
            sp.History.Select(p => $"{DateTime.Now:mm:ss fff} {p}"),
            sp.IsConnected.Select(p => $"{DateTime.Now:mm:ss fff} State: {p}"),
            sp.Received.Select(p => $"{DateTime.Now:mm:ss fff} Receive : {p}")
        )
        .Subscribe(p =>
        {
            Console.WriteLine(p);
        });

    Observable.Interval(TimeSpan.FromSeconds(3))
        .Subscribe(async _ =>
        {
            await sp.SendAsync(new CPContext("AA00 0a10 afBB", "AA00$2$1BB", 256));
        });

##### 2.ProtocolDeviceSimulator

    var simulator = new ProtocolDeviceSimulator();

    simulator.ConnectionState.Select(p => $"@ {DateTime.Now:mm:ss fff} State : {p}").Subscribe(p => msg.Add(p));
    simulator.Received.Select(p => $"@ {DateTime.Now:mm:ss fff} Receive : {p.ToHexStr()}").Subscribe(p => msg.Add(p));
    simulator.History.Subscribe(p =>
    {
        msg.Add($"@ {DateTime.Now:mm:ss fff} {p}");
        rst.Add(p);
    });

    await simulator.OpenAsync();
     
    await simulator.SendAsync(new CPContext("AA01BB", "AA$1BB", 1000, "01"));
    await simulator.SendAsync(new CPContext("AB02BB", "AB$1BB", 1000, "02"));
    await simulator.SendAsync(new CPContext("AC03BB", "AC$1BB", 1000, "03"));
    await simulator.SendAsync(new CPContext("AD04BB", "AD$1BB", 1000, "04"));
    await simulator.SendAsync(new CPContext("AE05BB", "AE$1BB", 1000, "05"));
    await simulator.SendAsync(new CPContext("AF06BB", "AF$1BB", 1000, "06"));
  
    await simulator.CloseAsync();
    simulator.CleanUp();

    msg.Count.Should().Be(2 + 2 * 6 * loop);
    rst.TrueForAll(p => p.Status == CPContextStatus.Success);

### Todos
    ~~byte[]分包~~
    ~~SerialPortProtocol_GS~~
    ~~TaskQueue(实现思路有问题)~~

    SerialPortProtocol_SPS : linux环境测试
    TcpClientProtocol_SST : 待真机测试
    TcpServerProtocol_SST : 待真机测试 
