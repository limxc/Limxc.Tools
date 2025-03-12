using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
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

        private bool IsAdministrator()
        {
            bool result;
            try
            {
                var identity = WindowsIdentity.GetCurrent();
                var principal = new WindowsPrincipal(identity);
                result = principal.IsInRole(WindowsBuiltInRole.Administrator);
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
                .GroupBy(p => p.index)
                .Select(p =>
                {
                    var idx = p.Key;
                    return (idx, p.FirstOrDefault(x => x.type == "A").portName.ToUpper(),
                        p.FirstOrDefault(x => x.type == "B").portName.ToUpper());
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
        ///     创建物理串口监听, 需要提前手动创建虚拟串口对
        ///     串口助手 <-> 虚拟串口1 <-> 虚拟串口2 <-> 监听转发 <-> 物理串口({physicalPortName}) <-> 串口助手
        /// </summary>
        /// <param name="virtualPortName"></param>
        /// <param name="physicalPortName"></param>
        /// <param name="baudRate"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<(IObservable<(bool SendOrRecv, byte[] Data)>, IDisposable)> ForwardingAsync(
            string virtualPortName, string physicalPortName, int baudRate, bool checkPort = true)
        {
            var disposables = new CompositeDisposable();
            var subject = new Subject<(bool, byte[])>();

            virtualPortName = virtualPortName.ToUpper();

            if (checkPort)
            {
                var vspPairs = await ListAsync();
                if (!vspPairs.Any(p => p.PortName2.Contains(virtualPortName)))
                    throw new Exception($"串口名错误:{virtualPortName}");
            }

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
                //发送前等待50ms分包
                .Buffer(TimeSpan.FromMilliseconds(50))
                .Select(p => p.SelectMany(b => b).ToArray())
                .Where(p => p.Length > 0)
                .CallAsync(async p =>
                {
                    if (pss.IsConnected)
                    {
                        _logger?.Invoke($"@{DateTime.Now:HH:mm:ss fff} V->P : {p.ByteToHex()}");
                        await pss.SendAsync(p);
                        subject.OnNext((true, p));
                    }
                    else
                    {
                        _logger?.Invoke($"@{DateTime.Now:HH:mm:ss fff} V : {p.ByteToHex()}");
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
                        _logger?.Invoke($"@{DateTime.Now:HH:mm:ss fff} P->V : {p.ByteToHex()}");
                        await vss.SendAsync(p);
                        subject.OnNext((false, p));
                    }
                    else
                    {
                        _logger?.Invoke($"@{DateTime.Now:HH:mm:ss fff} P : {p.ByteToHex()}");
                    }
                })
                .Subscribe()
                .DisposeWith(disposables);

            vss.DisposeWith(disposables);
            pss.DisposeWith(disposables);
            subject.DisposeWith(disposables);

            return (subject.AsObservable(), disposables);
        }

        /// <summary>
        ///     创建物理串口监听, 自动创建虚拟串口对
        ///     串口助手 <-> 虚拟串口1 <-> 虚拟串口2 <-> 监听转发 <-> 物理串口({physicalPortName}) <-> 串口助手
        /// </summary>
        /// <param name="physicalPortName"></param>
        /// <param name="baudRate"></param>
        /// <param name="comStart">100~200</param>
        /// <returns></returns>
        public async Task<(IObservable<(bool SendOrRecv, byte[] Data)>, IDisposable)> MonitoringAsync(
            string physicalPortName, int baudRate, int comStart = 100)
        {
            var com1 = $"COM{comStart}";
            var com2 = $"COM{comStart + 1}";

            var allPairs = await ListAsync();
            _logger?.Invoke("虚拟串口列表");
            foreach (var item in allPairs.Select(p => $"    {p.Index} : {p.PortName1} <-> {p.PortName2}"))
                _logger?.Invoke(item);

            var pair = allPairs.FirstOrDefault(p => p.PortName1 == com1 && p.PortName2 == com2);
            var createdBefore = true;
            if (pair == default)
            {
                createdBefore = false;
                _logger?.Invoke($"创建虚拟串口对: {com1} - {com2}");
                await CreateVirtualSerialPortAsync(com1, com2);
                pair = (await ListAsync()).FirstOrDefault(p => p.PortName1 == com1 && p.PortName2 == com2);
            }

            _logger?.Invoke(
                $"串口助手 <-> 虚拟串口1({com1}) <-> 虚拟串口2({com2}) <-> 监听转发 <-> 物理串口({physicalPortName}) <-> 串口助手");

            var (obs, dis) = await ForwardingAsync(com2, physicalPortName, baudRate);

            async void Dispose()
            {
                try
                {
                    dis.Dispose();
                    if (!createdBefore)
                    {
                        await RemoveVirtualSerialPortAsync(pair.Index);
                        _logger?.Invoke($"删除虚拟串口对: {com1} - {com2}");
                    }
                }
                catch (Exception e)
                {
                    _logger?.Invoke(e);
                }
            }

            return (obs, Disposable.Create(Dispose));
        }


        /// <summary>
        ///     模拟两组虚拟串口监听 COM(comStart) <-> COM(comStart+3)
        ///     串口助手 <-> 虚拟串口A1 <-> 虚拟串口A2 <-> 监听转发 <-> 模拟物理串口(虚拟串口B1) <-> 模拟物理串口(虚拟串口B2) <-> 串口助手
        /// </summary>
        /// <param name="baudRate"></param>
        /// <param name="comStart">200 ~ 250</param>
        /// <returns></returns>
        public async Task<(IObservable<(bool SendOrRecv, byte[] Data)>, IDisposable)> SimulateAsync(
            int baudRate, int comStart = 250)
        {
            var com1 = $"COM{comStart}";
            var com2 = $"COM{comStart + 1}";
            var com3 = $"COM{comStart + 2}";
            var com4 = $"COM{comStart + 3}";

            var allPairs = await ListAsync();
            _logger?.Invoke("虚拟串口列表");
            foreach (var item in allPairs.Select(p => $"    {p.Index} : {p.PortName1} <-> {p.PortName2}"))
                _logger?.Invoke(item);

            var pair1 = allPairs.FirstOrDefault(p => p.PortName1 == com1 && p.PortName2 == com2);
            var createdBefore1 = true;
            if (pair1 == default)
            {
                createdBefore1 = false;
                _logger?.Invoke($"创建虚拟串口对: {com1} - {com2}");
                await CreateVirtualSerialPortAsync(com1, com2);
                pair1 = (await ListAsync()).FirstOrDefault(p => p.PortName1 == com1 && p.PortName2 == com2);
            }

            var pair2 = allPairs.FirstOrDefault(p => p.PortName1 == com3 && p.PortName2 == com4);
            var createdBefore2 = true;
            if (pair2 == default)
            {
                createdBefore2 = false;
                _logger?.Invoke($"创建虚拟串口对: {com3} - {com4}");
                await CreateVirtualSerialPortAsync(com3, com4);
                pair2 = (await ListAsync()).FirstOrDefault(p => p.PortName1 == com3 && p.PortName2 == com4);
            }

            _logger?.Invoke(
                $"串口助手 <-> 虚拟串口1({com1}) <-> 虚拟串口2({com2}) <-> 监听转发 <-> 模拟物理串口({com3}) <-> 模拟物理串口({com4}) <-> 串口助手");
            allPairs = await ListAsync();
            allPairs.Dump();
            var (obs, dis) = await ForwardingAsync(com2, com3, baudRate, false);

            async void Dispose()
            {
                try
                {
                    dis.Dispose();
                    await RemoveAllVirtualSerialPortAsync();
                    if (!createdBefore1)
                    {
                        await RemoveVirtualSerialPortAsync(pair1.Index);
                        _logger?.Invoke($"删除虚拟串口对: {com1} - {com2}");
                    }

                    if (!createdBefore2)
                    {
                        await RemoveVirtualSerialPortAsync(pair2.Index);
                        _logger?.Invoke($"删除虚拟串口对: {com3} - {com4}");
                    }
                }
                catch (Exception e)
                {
                    _logger?.Invoke(e);
                }
            }

            return (obs, Disposable.Create(Dispose));
        }
    }
}