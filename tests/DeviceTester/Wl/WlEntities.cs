using Limxc.Tools.Common;
using System;
using System.Threading.Tasks;

namespace DeviceTester.Wl
{
    public static class WlCommands
    {
        public static string 旋转电极控制(bool doing) => $"T{(doing ? 0 : 1)}\r\n";

        /// <summary>
        /// -2048mv(0) ~ 2048mv(4096)
        /// </summary>
        /// <param name="mv"></param>
        /// <returns></returns>
        public static string 电极电位设置功能(int mv) => $"R{mv + 2048:0000}\r\n";

        public static string 初始富集电位设置(int mv) => $"L{mv + 2048:0000}\r\n";

        public static string 终止净化电位设置(int mv) => $"M{mv + 2048:0000}\r\n";

        public static string 扫描速度电压设置(int mv) => $"J{mv + 2048:0000}\r\n";

        public static string 启动电位溶出() => "X\r\n";

        public static string 取回数据(int index) => $"Y{index:000}\r\n";

        public static string 微分开关(bool open) => $"X{(open ? 1 : 0)}\r\n";

        /// <summary>
        /// 0~7
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        public static string 灵敏度调节(int level) => $"B{level}\r\n";
    }

    public class WlTask
    {
        public string Name { get; set; }
        public TaskQueue<bool> Tasks { get; set; }
    }

    public static class WlCompCmds
    {
        public static TaskQueue<bool> 测试电机(Func<string, Task<bool>> send)
        {
            var queue = new TaskQueue<bool>();
            queue.Add(async token =>
            {
                await send(WlCommands.旋转电极控制(true));
                for (int i = 0; i < 3; i++)
                {
                    token.ThrowIfCancellationRequested();
                    await Task.Delay(1000);
                }
                await send(WlCommands.旋转电极控制(false));
                return true;
            });
            return queue;
        }

        public static TaskQueue<bool> 测量(Func<string, Task<bool>> send)
        {
            var queue = new TaskQueue<bool>();
            //1、送初始电位、终止电位以及量程选择为最低档（B0）
            queue.Add(_ => send(WlCommands.灵敏度调节(0)));

            //2、电极净化：送0V电位，电机开始旋转，维持10s
            queue.Add(_ => send(WlCommands.终止净化电位设置(0)));
            queue.Add(async token =>
            {
                await send(WlCommands.旋转电极控制(true));
                for (int i = 0; i < 10; i++)
                {
                    token.ThrowIfCancellationRequested();
                    await Task.Delay(1000);
                }
                await send(WlCommands.旋转电极控制(false));
                return true;
            });

            //3、电极富集：送 - 1.2V电位，电机保持旋转100s
            queue.Add(_ => send(WlCommands.电极电位设置功能(1200)));
            queue.Add(async token =>
            {
                await send(WlCommands.旋转电极控制(true));
                for (int i = 0; i < 100; i++)
                {
                    token.ThrowIfCancellationRequested();
                    await Task.Delay(1000);
                }
                await send(WlCommands.旋转电极控制(false));
                return true;
            });

            //4、避开氢波：送初始电位，电机保持旋转，保持4s,量程B命令切换到使用档。
            //5、避开氢波：电机停转，保持静止4s
            //6、开始扫描：扫描时间为：（初始电位 - 终止电位）/ 扫描速度 + 1s，扫描速度固定为250mv / s
            //7、电极保护：送电位0V，送B0；
            //8、送Y命令取数，取数量为：（初始电位 - 终止电位）/ 扫描速度 / 采样时间间隔
            //9、谱图处理。

            return queue;
        }

        public static TaskQueue<bool> 镀汞(Func<string, Task<bool>> send)
        {
            return 测量(send).Combine(测量(send), 测量(send), 测量(send));
        }
    }
}