using System.Collections.Generic;

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

        public static List<WlCommand> Commands { get; } = new List<WlCommand>()
        {
            new WlCommand("旋转电极控制",旋转电极控制(true)),
            new WlCommand("电极电位设置功能",电极电位设置功能(0)),
            new WlCommand("初始富集电位设置",初始富集电位设置(0)),
            new WlCommand("终止净化电位设置",终止净化电位设置(0)),
            new WlCommand("扫描速度电压设置",扫描速度电压设置(0)),
            new WlCommand("启动电位溶出",启动电位溶出()),
            new WlCommand("取回数据",取回数据(0)),
            new WlCommand("微分开关",微分开关(true)),
            new WlCommand("灵敏度调节",灵敏度调节(1)),
        };
    }

    public class WlCommand
    {
        public WlCommand(string name, string command)
        {
            Name = name;
            Command = command;
        }

        public string Name { get; set; }
        public string Command { get; set; }
    }
}