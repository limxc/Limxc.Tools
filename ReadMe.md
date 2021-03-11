### Limxc.Tools
##### 1.Pipeline

    var pipe = new PipeBuilder<PipeTestContext>()
            .Use(c =>
            {
                c.Msg += "_1";
                c.Value += 1;
            }, "process1")
            .Use(async (c, t) =>//100ms
            {
                await Task.Delay(1000);
                if (t.IsCancellationRequested) 
                    return;
                c.Msg += "_2";
                c.Value += 2;
            }, "process2")
            .Use(c =>//1100ms
            {
                c.Msg += "_3"; 
                c.Value += 3;
            }) 
            .Build();
    
    //Result: Test_1_2_3    Snapshot: original process1 process2
    var rst = await pipe.RunAsync(new PipeTestContext("Test", 0), CancellationToken.None); 
    
    public class PipeTestContext
    {
        public string Msg { get; set; }
        public double Value { get; set; }
    
        public PipeTestContext(string msg, double value)
        {
            Msg = msg;
        }
    
        public override string ToString()
        {
            return $"{Msg} | {Value}";
        }
    }

### Limxc.Tools.DeviceComm
##### 1.SerialPort 

    var sp = new SerialPortProtocol();
    
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
    await simulator.CloseAsync();
    
    simulator.CleanUp(); 

### Schedule
Test TcpClientProtocol  
Test TcpServerProtocol

### Recommended Packages 
##### Communication
[GodSharp.SerialPort](https://github.com/godsharp/GodSharp.SerialPort)  
[SerialPortStream](https://github.com/jcurl/serialportstream)  
[SuperSimpleTcp](https://github.com/jchristn/simpletcp)  
[MQTTnet](https://github.com/chkr1011/MQTTnet)  
##### Http
[Flurl](https://flurl.dev/)  
[Refit](https://reactiveui.github.io/refit/)  
##### Mvvm
[ReactiveUI](https://reactiveui.net/)  
##### DeepCopy
[DeepCloner](https://github.com/force-net/DeepCloner)  
##### UnitTest
[AutoBogus](https://github.com/nickdodd79/AutoBogus)  
[FakeItEasy](https://fakeiteasy.github.io/)  
[FluentAssertions](https://www.fluentassertions.com/)  
[xunit](https://github.com/xunit/xunit)  
##### others
[Newtonsoft.Json](https://www.newtonsoft.com/json)  
[Serilog](https://github.com/serilog/)  
[SharpConfig](https://github.com/cemdervis/SharpConfig)  

### Thanks to [JetBrains](https://jb.gg/OpenSource) ！  
<img src="https://www.jetbrains.com/shop/static/images/jetbrains-logo-inv.svg" height="100">     

### License
    MIT