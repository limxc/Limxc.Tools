# Limxc.Tools

```
├─Bases(基类)
│ └─Communication(通讯对象封装)
├─Common(概率随机,任务队列等)
├─Extensions(通用扩展方法)
│ └─Communication(通讯相关扩展方法)
├─Pipeline(仿 ASP.NET Core Middleware)
│ ├─Builder
│ └─Context
├─Specification(规约模式)
└─Utils(集成 Frank A. Krueger 的绑定辅助类,内存文件映射)
```

# Limxc.Tools.Contract

```
├─Common
├─Interfaces
```

# Limxc.Tools.Core

```
├─CrcCSharp(集成meetanthony/crccsharp)
├─Services(部分Limxc.Tools.Contract.Interfaces实现)
└─SharpConfig(集成cemdervis/SharpConfig)
```

# Limxc.Tools.DeviceComm

```
├─Abstractions
├─Extensions
├─MQTT
├─Protocol
└─Utils
```

# Limxc.Tools.SerialPort

```
│  ISerialPortService.cs
│  SerialPortService.cs
│  SerialPortServiceSimulator.cs
│  SerialPortSetting.cs
```

### Recommended Packages

##### Communication

[SuperSimpleTcp](https://github.com/jchristn/simpletcp)

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
