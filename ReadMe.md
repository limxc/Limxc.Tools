[DeepWiki Refence](https://deepwiki.com/limxc/Limxc.Tools/3.1-limxc.tools-base-library)

# Limxc.Tools

```
├─Bases(常用基类)
│ └─Communication(通讯对象封装)
├─Common(概率随机,任务队列等)
├─Extensions(通用扩展方法)
│ └─Communication(通讯相关扩展方法)
├─Pipeline(仿 ASP.NET Core Middleware)
│ ├─Builder
│ └─Context
├─Specification(规约模式)
└─Utils(修改自"Frank A. Krueger"的绑定辅助类,内存文件映射)
```

# Limxc.Tools.Contract (常用服务契约)

```
├─Interfaces()
```

# Limxc.Tools.Core

```
├─Common(工具类)
├─CrcCSharp(修改自"meetanthony/crccsharp")
├─Services(Limxc.Tools.Contract.Interfaces的部分实现)
└─SharpConfig(修改自"cemdervis/SharpConfig")
```

# Limxc.Tools.DeviceComm

```
├─Abstractions(通用通讯接口, 见Limxc.Tools.Bases.Communication)
├─Extensions
├─MQTT(已移除, 待单独实现)
├─Protocol(串口, 特殊模式下的TCP)
└─Utils(简化版, 以被Limxc.Tools.SerialPort替代)
```

# Limxc.Tools.SerialPort
简化实现方式, 约定配置基类, 通过各种方式读取保存
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
