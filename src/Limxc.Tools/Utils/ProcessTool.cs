using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Limxc.Tools.Utils
{
    public static class ProcessTool
    {
        // ReSharper disable once InconsistentNaming
        private const int WS_SHOWNORMAL = 1;

        [DllImport("User32.dll")]
        private static extern bool ShowWindowAsync(IntPtr hWnd, int cmdShow);

        [DllImport("User32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        /// <summary>
        ///     获取当前运行实例
        /// </summary>
        /// <returns></returns>
        public static Process GetRunningInstance()
        {
            var currentProcess = Process.GetCurrentProcess();

            var cp = currentProcess.MainModule?.FileName;

            return Process
                .GetProcessesByName(currentProcess.ProcessName)
                .Where(p => p.Id != currentProcess.Id)
                .Where(p => p.MainModule != null)
                .FirstOrDefault(p => p.MainModule.FileName.Equals(cp, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        ///     查找进程
        /// </summary>
        /// <param name="path">exe完整路径</param>
        /// <returns></returns>
        public static Process[] FindProcess(string path)
        {
            var name = Path.GetFileNameWithoutExtension(path);
            return Process.GetProcessesByName(name)
                .Where(p => p.MainModule != null)
                .Where(p => string.Equals(p.MainModule.FileName, path, StringComparison.OrdinalIgnoreCase))
                .OrderBy(p => p.StartTime)
                .ToArray();
        }

        /// <summary>
        ///     启动或前台显示
        /// </summary>
        /// <param name="path">exe完整路径</param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static async Task<Process> StartOrShowProcess(string path, string args = "")
        {
            var process = FindProcess(path).FirstOrDefault();
            if (process != null)
                return process.ShowRunningInstance();

            return await StartApplication(path, args);
        }

        /// <summary>
        ///     启动应用
        /// </summary>
        /// <param name="path">exe完整路径</param>
        /// <param name="args"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static async Task<Process> StartApplication(string path, string args = "")
        {
            if (!File.Exists(path))
                throw new ArgumentException($"应用路径错误: {path}");

            var p = new Process();

            p.StartInfo.FileName = path;
            p.StartInfo.Arguments = args;
            p.StartInfo.WorkingDirectory = Path.GetDirectoryName(path) ?? string.Empty;
            p.Start();

            while (string.IsNullOrEmpty(p.MainWindowTitle))
            {
                await Task.Delay(100);
                p.Refresh();
            }

            return p;
        }

        /// <summary>
        ///     前台显示
        /// </summary>
        /// <param name="process"></param>
        /// <returns></returns>
        public static Process ShowRunningInstance(this Process process)
        {
            ShowWindowAsync(process.MainWindowHandle, WS_SHOWNORMAL);
            SetForegroundWindow(process.MainWindowHandle);

            return process;
        }


        /// <summary>
        ///     执行一个控制台程序，并获取在控制台返回的数据
        /// </summary>
        /// <param name="command">dos/cmd命令</param>
        /// <param name="waitMs">等待执行时间毫秒值，默认不等待</param>
        /// <returns>控制台输出信息</returns>
        /// <exception cref="SystemException">
        ///     尚未设置进程 <see cref="P:System.Diagnostics.Process.Id" />，而且不存在可从其确定
        ///     <see cref="P:System.Diagnostics.Process.Id" /> 属性的 <see cref="P:System.Diagnostics.Process.Handle" />。- 或 -没有与此
        ///     <see cref="T:System.Diagnostics.Process" /> 对象关联的进程。- 或 -您正尝试为远程计算机上运行的进程调用
        ///     <see cref="M:System.Diagnostics.Process.WaitForExit(System.Int32)" />。此方法仅对本地计算机上运行的进程可用。
        /// </exception>
        /// <exception cref="Exception">命令参数无效，必须传入一个控制台能被cmd.exe可执行程序; 如：ping 127.0.0.1</exception>
        public static string RunCmd(string command, int waitMs = 0)
        {
            var output = "";
            if (!string.IsNullOrEmpty(command))
            {
                using (var process = new Process())
                {
                    var startInfo = new ProcessStartInfo
                    {
                        FileName = "cmd.exe", //设定需要执行的命令程序

                        //以下是隐藏cmd窗口的方法
                        Arguments = "/c" + command, //设定参数，要输入到命令程序的字符，其中"/c"表示执行完命令后马上退出
                        UseShellExecute = false, //不使用系统外壳程序启动
                        RedirectStandardInput = false, //不重定向输入
                        RedirectStandardOutput = true, //重定向输出，而不是默认的显示在dos控制台上
                        CreateNoWindow = true //不创建窗口
                    }; //创建进程时使用的一组值，如下面的属性

                    process.StartInfo = startInfo;
                    if (process.Start()) //开始进程
                    {
                        if (waitMs == 0)
                            process.WaitForExit();
                        else
                            process.WaitForExit(waitMs);

                        output = process.StandardOutput.ReadToEnd(); //读取进程的输出
                    }
                }


                return output;
            }

            throw new Exception("命令参数无效，必须传入一个控制台能被cmd.exe可执行程序;\n如：ping 127.0.0.1");
        }
    }
}