using System;
using System.IO;

namespace Limxc.Tools.Core.Utils
{
    public static class EnvPath
    {
        public static string OutputDir => Path.Combine(BaseDirectory, "Outputs");
        public static string ReportDir => Path.Combine(BaseDirectory, "Reports");
        public static string ResDir => Path.Combine(BaseDirectory, "Resources");
        public static string DatabaseDir => Path.Combine(BaseDirectory, "Database");

        public static string BaseDirectory
        {
            get
            {
                /*
                    获取基目录，它由程序集冲突解决程序用来探测程序集。
                    Debug.WriteLine($"*AppDomain.CurrentDomain.BaseDirectory : {AppDomain.CurrentDomain.BaseDirectory}");

                    可获得当前执行的exe的路径。
                    Debug.WriteLine($"Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName) : {Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName)}");

                    获取和设置当前目录（即该进程从中启动的目录）的完全限定路径。(备注: 按照定义，如果该进程在本地或网络驱动器的根目录中启动，则此属性的值为驱动器名称后跟一个尾部反斜杠（如“C:\”）。如果该进程在子目录中启动，则此属性的值为不带尾部反斜杠的驱动器和子目录路径[如“C:\mySubDirectory”])。
                    Debug.WriteLine($"Environment.CurrentDirectory : {Environment.CurrentDirectory}");

                    获取应用程序的当前工作目录
                    Debug.WriteLine($"Directory.GetCurrentDirectory(): {Directory.GetCurrentDirectory()}");

                    获取启动了应用程序的可执行文件的路径，不包括可执行文件的名称。
                    Debug.WriteLine($"System.Windows.Forms.Application.StartupPath : {System.Windows.Forms.Application.StartupPath}");

                    获取启动了应用程序的可执行文件的路径，包括可执行文件的名称。
                    Debug.WriteLine($"System.Windows.Forms.Application.ExecutablePath : {System.Windows.Forms.Application.ExecutablePath}");

                    获取或设置包含该应用程序的目录的名称。
                    Debug.WriteLine($"AppDomain.CurrentDomain.SetupInformation.ApplicationBase : {AppDomain.CurrentDomain.SetupInformation.ApplicationBase}");
                */
                var dir = AppDomain.CurrentDomain.BaseDirectory;

                return dir;
            }
        }
    }
}