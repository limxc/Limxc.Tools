using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CliWrap;
using CliWrap.Buffered;
using Limxc.Tools.Core.Common;
using Limxc.Tools.Extensions;
using Limxc.Tools.Extensions.Communication;
using Limxc.Tools.SerialPort;

// ReSharper disable InvalidXmlDocComment
// ReSharper disable InconsistentNaming

namespace Limxc.Tools.Emulator
{
    public class VirtualCom
    {
        private readonly Action<object> _logger = o => Debug.WriteLine(o);
        private readonly string _path;
        private readonly string _regex_Index = @"(?<=CNC[A|B])\d";
        private readonly string _regex_PortName = @"(?<=PortName=)[\w-]+(?=,|$)";
        private readonly string _regex_Type = @"(?<=CNC)[A-Z]";
        private readonly string _setupc;

        public VirtualCom(string path)
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                throw new Exception("仅支持Windows");

            if (!IsAdministrator())
                throw new Exception("需要管理员权限");

            if (Environment.Is64BitOperatingSystem)
                path = Path.Combine(path, "com0com", "x64");
            else
                path = Path.Combine(path, "com0com", "i386");

            if (!Directory.Exists(path)) throw new DirectoryNotFoundException();
            _path = path;

            _setupc = Path.Combine(path, "setupc.exe");
            if (!File.Exists(_setupc)) throw new FileNotFoundException("未找到com0com执行文件setupc.exe");
        }

        public VirtualCom(string path, Action<object> logger) : this(path)
        {
            _logger = logger;
        }

        public bool IsAdministrator()
        {
            bool result;
            try
            {
#pragma warning disable CA1416
                var identity = WindowsIdentity.GetCurrent();
                var principal = new WindowsPrincipal(identity);
                result = principal.IsInRole(WindowsBuiltInRole.Administrator);
#pragma warning restore CA1416
            }
            catch
            {
                result = false;
            }

            return result;
        }

        private Command CreateCommand(params string[] values)
        {
            return Cli.Wrap(_setupc)
                .WithWorkingDirectory(_path)
                .WithArguments(args => args
                    .Add($"--output \"{Path.Combine(EnvPath.Default.LogFolder(), "com0com.log")}\"")
                    .Add("--silent")
                    .Add(values)
                );
        }

        private async Task<string> ExecAsync(Command cmd)
        {
            var result = await cmd.ExecuteBufferedAsync();
            return result.StandardOutput;
        }

        public async Task<(int Index, string PortName1, string PortName2)[]> ListAsync()
        {
            var result = await CreateCommand("list").ExecuteBufferedAsync();
            var lines = result.StandardOutput.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

            return lines
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .Select(p =>
                {
                    var index = Convert.ToInt32(Regex.Match(p, _regex_Index).Value);
                    var type = Regex.Match(p, _regex_Type).Value;
                    var portName = Regex.Match(p, _regex_PortName).Value;
                    return (index, type, portName);
                })
                .GroupBy(p => p.Item1)
                .Select(p =>
                {
                    var idx = p.Key;
                    return (idx, p.FirstOrDefault(x => x.Item2 == "A").portName,
                        p.FirstOrDefault(x => x.Item2 == "B").portName);
                })
                .ToArray();
        }

        public async Task<string> CreateVirtualSerialPortAsync(string first = "COM201", string second = "COM202")
        {
            var cmd = CreateCommand($"install PortName={first},EmuBR=yes PortName={second},EmuBR=yes");
            return await ExecAsync(cmd);
        }

        public async Task<string> RemoveVirtualSerialPortAsync(int index)
        {
            var cmd = CreateCommand("remove", index.ToString());

            return await ExecAsync(cmd);
        }

        public async Task<string> RemoveAllVirtualSerialPortAsync()
        {
            var sb = new StringBuilder();

            var idx = (await ListAsync()).Select(p => p.Index).Distinct();

            foreach (var item in idx) sb.AppendLine(await RemoveVirtualSerialPortAsync(item));

            return sb.ToString();
        }

        /// <summary>
        ///     上位机 <-> 物理串口
        ///     上位机 <-> 虚拟串口1(first) <-> 虚拟串口2(second) <-> 监听转发 <-> 物理串口
        /// </summary>
        /// <param name="virtualPortName"></param>
        /// <param name="physicalPortName"></param>
        /// <param name="baudRate"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<CompositeDisposable> ForwardingAsync(string virtualPortName, string physicalPortName,
            int baudRate)
        {
            var disposables = new CompositeDisposable();

            virtualPortName = virtualPortName.ToUpper();
            var vspPairs = await ListAsync();
            if (!vspPairs.Any(p => p.PortName2.Contains(virtualPortName)))
                throw new Exception($"串口名错误:{virtualPortName}");

            var vss = new SerialPortService();
            var pss = new SerialPortService();

            vss.ConnectionState
                .CombineLatest(pss.ConnectionState)
                .DistinctUntilChanged()
                .Subscribe(p =>
                {
                    _logger?.Invoke($"连接状态: 虚拟串口{(p.First ? "√" : "X")} | 物理串口{(p.Second ? "√" : "X")}");
                })
                .DisposeWith(disposables);

            //监听虚拟串口并记录, 转发到物理串口
            vss.Start(new SerialPortSetting
            {
                PortName = virtualPortName,
                BaudRate = baudRate
            });
            vss.Received
                .CallAsync(async p =>
                {
                    if (pss.IsConnected)
                    {
                        _logger?.Invoke($"@{DateTime.Now:mm:ss} V->P : {p.ByteToHex()}");
                        await pss.SendAsync(p);
                    }
                    else
                    {
                        _logger?.Invoke($"@{DateTime.Now:mm:ss} V : {p.ByteToHex()}");
                    }
                })
                .Subscribe()
                .DisposeWith(disposables);

            //监听物理串口并记录, 转发到虚拟串口
            pss.Start(new SerialPortSetting
            {
                PortName = physicalPortName,
                BaudRate = baudRate
            });
            pss.Received
                .CallAsync(async p =>
                {
                    if (vss.IsConnected)
                    {
                        _logger?.Invoke($"@{DateTime.Now:mm:ss} P->V : {p.ByteToHex()}");
                        await vss.SendAsync(p);
                    }
                    else
                    {
                        _logger?.Invoke($"@{DateTime.Now:mm:ss} P : {p.ByteToHex()}");
                    }
                })
                .Subscribe()
                .DisposeWith(disposables);

            vss.DisposeWith(disposables);
            pss.DisposeWith(disposables);

            return disposables;
        }

        /// <summary>
        ///     创建两组虚拟串口模拟 COM(comStart) <-> COM(comStart+3)
        /// </summary>
        /// <param name="baudRate"></param>
        /// <param name="comStart">200 ~ 250</param>
        /// <returns></returns>
        public async Task<IDisposable> TestAsync(int baudRate, int comStart = 250)
        {
            _logger?.Invoke("移出所有虚拟串口");
            await RemoveAllVirtualSerialPortAsync();

            _logger?.Invoke("创建虚拟串口");
            await CreateVirtualSerialPortAsync($"COM{comStart}", $"COM{comStart + 1}");

            _logger?.Invoke("模拟物理串口");
            await CreateVirtualSerialPortAsync($"COM{comStart + 2}", $"COM{comStart + 3}");

            var list = await ListAsync();
            _logger?.Invoke("虚拟串口列表");
            foreach (var item in list.Select(p => $"    {p.Index} : {p.PortName1} <-> {p.PortName2}"))
                _logger?.Invoke(item);

            _logger?.Invoke(
                $"串口助手 <-> 虚拟串口1(COM{comStart}) <-> 虚拟串口2(COM{comStart + 1}) <-> 监听转发 <-> 模拟物理串口(COM{comStart + 2}) <-> 模拟物理串口(COM{comStart + 3}) <-> 串口助手");

            var dis = await ForwardingAsync($"COM{comStart + 1}", $"COM{comStart + 2}", baudRate);

            async void Dispose()
            {
                try
                {
                    dis.Dispose();
                    await RemoveAllVirtualSerialPortAsync();
                    list = await ListAsync();
                    _logger?.Invoke($"移出所有虚拟串口, 剩余:{list.Count()}");
                }
                catch (Exception e)
                {
                    _logger?.Invoke(e);
                }
            }

            return Disposable.Create(Dispose);
        }

        /// <summary>
        ///     创建串口监听
        /// </summary>
        /// <param name="physicalPortName"></param>
        /// <param name="baudRate"></param>
        /// <param name="comStart">100~200</param>
        /// <returns></returns>
        public async Task<IDisposable> MonitoringAsync(string physicalPortName, int baudRate, int comStart = 100)
        {
            _logger?.Invoke("移出所有虚拟串口");
            await RemoveAllVirtualSerialPortAsync();

            _logger?.Invoke("创建虚拟串口");
            await CreateVirtualSerialPortAsync($"COM{comStart}", $"COM{comStart + 1}");

            var list = await ListAsync();
            _logger?.Invoke("虚拟串口列表");
            foreach (var item in list.Select(p => $"    {p.Index} : {p.PortName1} <-> {p.PortName2}"))
                _logger?.Invoke(item);

            _logger?.Invoke(
                $"串口助手 <-> 虚拟串口1(COM{comStart}) <-> 虚拟串口2(COM{comStart + 1}) <-> 监听转发 <-> 物理串口({physicalPortName}) <-> 串口助手");

            var dis = await ForwardingAsync($"COM{comStart + 1}", physicalPortName, baudRate);

            async void Dispose()
            {
                try
                {
                    dis.Dispose();
                    await RemoveAllVirtualSerialPortAsync();
                    list = await ListAsync();
                    _logger?.Invoke($"移出所有虚拟串口, 剩余:{list.Count()}");
                }
                catch (Exception e)
                {
                    _logger?.Invoke(e);
                }
            }

            return Disposable.Create(Dispose);
        }
    }
}