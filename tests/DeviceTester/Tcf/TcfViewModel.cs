using Limxc.Tools.DeviceComm.Protocol;
using Limxc.Tools.Extensions;
using Limxc.Tools.Extensions.DevComm;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ReactiveUI.Validation.Abstractions;
using ReactiveUI.Validation.Contexts;
using ReactiveUI.Validation.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DeviceTester.Tcf
{
    public class TcfViewModel : ReactiveObject, IValidatableViewModel
    {
        private TcfCRC _crc;

        public ValidationContext ValidationContext { get; } = new ValidationContext();
        private CompositeDisposable _disposables;

        private IProtocol _protocol, _emptyProtocol;

        public void SetProtocol(IProtocol protocol = null)
        {
            CleanUp();

            _disposables = new CompositeDisposable();

            _protocol = protocol ?? _emptyProtocol;

            Init();
        }

        public TcfViewModel()
        {
            _crc = new TcfCRC();
            _emptyProtocol = new EmptyProtocol();

            SetProtocol();
        }

        private void Init()
        {
            #region Connection

            _protocol.ConnectionState
                .SubscribeOn(RxApp.MainThreadScheduler)
                .ToPropertyEx(this, vm => vm.IsConnected)
                .DisposeWith(_disposables);

            Connect = ReactiveCommand.CreateFromTask(() => _protocol.OpenAsync()).DisposeWith(_disposables);

            var autoReconnect = Observable.Interval(TimeSpan.FromSeconds(1))
                .Where(_ => !IsConnected)
                .Select(_ => Unit.Default);

            this.WhenAnyValue(p => p.EnableAutoReconnect)
                .Select(enabled => enabled ? autoReconnect : Observable.Empty<Unit>())
                .Switch()
                .Debug("try connect.")
                .InvokeCommand(Connect)
                .DisposeWith(_disposables);

            #endregion Connection

            #region Validation

            var measureParams = this.WhenAnyValue(
                x => x.IsConnected,
                x => x.Step,
                x => x.Id,
                x => x.Name,
                x => x.Height,
                x => x.Weight,
                x => x.Age,
                x => x.Gender,
                (isConnected, step, id, name, height, weight, age, gender) =>
                {
                    //if (Debugger.IsAttached)
                    //    return isConnected && Regex.IsMatch(id, @"^[A-Za-z0-9]{1,18}$");

                    return
                        isConnected &&
                        step >= 2 &&//状态2才能开始测量
                        Regex.IsMatch(id, @"^[A-Za-z0-9]{1,18}$") &&
                        weight > 0;
                }).ObserveOn(RxApp.MainThreadScheduler);

            this.ValidationRule(measureParams, "请检查设备连接,测量体重并输入1-18位Id.\n")
                .DisposeWith(_disposables);

            #endregion Validation

            #region Device Commands

            var canMeasure = measureParams;

            Measure = ReactiveCommand.CreateFromTask<Unit, bool>(async _ =>
            {
                if (!await Send(TcfCommands.SendId(Id)))
                    return false;

                await Task.Delay(100);

                if (!await Send(TcfCommands.SendHeight(Height)))
                    return false;

                await Task.Delay(100);

                if (!await Send(TcfCommands.SendAge(Age)))
                    return false;

                await Task.Delay(100);

                if (!await Send(TcfCommands.SendGender(Gender)))
                    return false;

                if (!await Send(TcfCommands.Measure()))
                    return false;

                return true;
            }, canMeasure)
            .DisposeWith(_disposables);

            GetResult = ReactiveCommand.CreateFromTask<Unit, bool>(_ => Send(TcfCommands.Read())).DisposeWith(_disposables);

            Reset = ReactiveCommand.CreateFromTask<Unit, bool>(_ => Send(TcfCommands.ReturnHome())).DisposeWith(_disposables);

            #endregion Device Commands

            #region Received Datas

            var received = _protocol.Received
                .Select(p => p.ToHexStr().HexToAscII())
                .SelectMany(p => p)
                .ParsePackage("\n".ToCharArray())
                .Select(p => new string(p))
                .Select(p =>
                {
                    var crc = p.Substring(0, 4);
                    var value = p.Substring(4);
                    var valueCrc = _crc.CRCEfficacy(value + "\n");
                    var eq = string.Equals(crc, valueCrc, StringComparison.OrdinalIgnoreCase);
                    if (!eq)
                        Debug.WriteLine($"*** Crc error: {crc} {valueCrc} {value}");
                    return (eq, value);
                })
                //.Debug("Received")
                .Where(p => p.eq)//只返回crc校验通过的数据
                .Select(p => p.value)
                .Debug("Received crc")
                .SubscribeOn(NewThreadScheduler.Default)
                .Publish()
                .RefCount();

            //Steps
            received
                .Where(p => p.StartsWith("*stp="))
                .Select(p => TryInt(p.Replace("*stp=", "")))
                .Where(p => p > 0)
                .ObserveOn(RxApp.MainThreadScheduler)
                .ToPropertyEx(this, vm => vm.Step)
                .DisposeWith(_disposables);

            //error
            received
                .Where(p => p.StartsWith("*err="))
                .Select(p =>
                {
                    switch (TryInt(p.Replace("*err=", "")))
                    {
                        case 1:
                            return "采集板通信错误";

                        case 2:
                            return "输入信息错误";

                        case 3:
                            return "接触阻抗异常";

                        default:
                            return "";
                    }
                })
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .ObserveOn(RxApp.MainThreadScheduler)
                .ToPropertyEx(this, vm => vm.Error)
                .DisposeWith(_disposables);

            //weight
            received
                .Where(p => Regex.IsMatch(p, @"^\d{1,3}\.\d$"))//x.x;xx.x;xxx.x
                .Select(p => TryDouble(p))
                .ObserveOn(RxApp.MainThreadScheduler)
                .ToPropertyEx(this, vm => vm.Weight)
                .DisposeWith(_disposables);

            //result
            received
                .Where(p => p.StartsWith("id="))
                .Select(p => new TcfResult(p))
                .ObserveOn(RxApp.MainThreadScheduler)
                .ToPropertyEx(this, vm => vm.Result)
                .DisposeWith(_disposables);

            #endregion Received Datas

            #region Debug

            received.Merge(_protocol.ConnectionState.Select(p => p ? "连接成功" : "连接断开"))
                .Timestamp()
                .Select(p => $"{p.Timestamp.LocalDateTime:HH:mm:ss fff} | {p.Value}")
                .Bucket(10)
                .SubscribeOn(NewThreadScheduler.Default)
                .ObserveOn(RxApp.MainThreadScheduler)
                .ToPropertyEx(this, vm => vm.ReceivedMsg)
                .DisposeWith(_disposables);

            this.WhenAnyValue(vm => vm.Result)
                .Where(p => p != null)
                .Select(p => p.GetResult().Select(r => $"{r.Key}:{r.Value}"))
                .SubscribeOn(NewThreadScheduler.Default)
                .ObserveOn(RxApp.MainThreadScheduler)
                .ToPropertyEx(this, vm => vm.ResultList)
                .DisposeWith(_disposables);

            //this.WhenAnyValue(vm => vm.IsConnected)
            //    .Debug("IsConnected")
            //    .Subscribe();

            Id = "123456";
            Name = "张三";

            #endregion Debug

            _protocol.ConnectionState.Where(p => p).Take(1).Select(_ => Unit.Default).InvokeCommand(Reset).DisposeWith(_disposables);

            Connect.Execute().Subscribe().Dispose();
        }

        #region Properties

        /// <summary>
        /// 外部数据
        /// </summary>
        [Reactive] public dynamic Data { get; set; }

        [Reactive] public string Id { get; set; } = "";
        [Reactive] public string Name { get; set; }
        [Reactive] public double Height { get; set; } = 90;
        [Reactive] public int Age { get; set; } = 5;
        [Reactive] public string Gender { get; set; } = "男";

        public bool IsConnected { [ObservableAsProperty] get; }

        [Reactive] public bool EnableAutoReconnect { get; set; }

        #region 返回信息

        /// <summary>
        /// 当前进行的步骤
        /// </summary>
        public int Step { [ObservableAsProperty] get; }

        /// <summary>
        /// 错误信息
        /// </summary>
        public string Error { [ObservableAsProperty] get; }

        /// <summary>
        /// 体重
        /// </summary>
        public double Weight { [ObservableAsProperty] get; }

        /// <summary>
        /// 最终结果
        /// </summary>
        public TcfResult Result { [ObservableAsProperty] get; }

        #endregion 返回信息

        #region Debug

        [ObservableAsProperty] public IEnumerable<string> ReceivedMsg { get; }
        [ObservableAsProperty] public IEnumerable<string> ResultList { get; }

        #endregion Debug

        #endregion Properties

        #region Commands

        public ReactiveCommand<Unit, bool> Connect { get; private set; }

        /// <summary>
        /// 拆开  每次修改 id height age gender 发送对应指令
        /// id : 1-18位
        /// height : 90.0-220.0
        /// age : 5-99
        /// gender : 男/女
        /// </summary>
        public ReactiveCommand<Unit, bool> Measure { get; private set; }

        public ReactiveCommand<Unit, bool> GetResult { get; private set; }

        public ReactiveCommand<Unit, bool> Reset { get; private set; }

        public ReactiveCommand<string, Unit> CreateConnection { get; private set; }

        #endregion Commands

        private Task<bool> Send(string cmd)
        {
            if (string.IsNullOrWhiteSpace(cmd) || _protocol == null)
                return Task.FromResult(false);

            var crc = _crc.CRCEfficacy(cmd);
            var bytes = (crc + cmd).AscIIToHex().ToByte();

            $"Send :{crc + cmd}".Debug();

            return _protocol.SendAsync(bytes);
        }

        private double TryDouble(string value, double def = 0) => double.TryParse(value, out double r) ? r : def;

        private int TryInt(string value, int def = 0) => int.TryParse(value, out int r) ? r : def;

        public void CleanUp()
        {
            _protocol?.Dispose();
            _disposables?.Dispose();
        }
    }
}