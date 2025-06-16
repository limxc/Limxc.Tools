using System;
using System.IO;

namespace Limxc.Tools.Core.Common
{
    /*
        获取基目录，它由程序集冲突解决程序用来探测程序集。
        Debug.WriteLine($"*AppDomain.CurrentDomain.BaseDirectory : {AppDomain.CurrentDomain.BaseDirectory}");

        可获得当前执行的exe的路径。
        Debug.WriteLine($"Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName) : {Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName)}");

        获取和设置当前目录（即该进程从中启动的目录）的完全限定路径。(备注: 按照定义，如果该进程在本地或网络驱动器的根目录中启动，则此属性的值为驱动器名称后跟一个尾部反斜杠（如“C:\”）
           。如果该进程在子目录中启动，则此属性的值为不带尾部反斜杠的驱动器和子目录路径[如“C:\mySubDirectory”])。
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

    public static class EnvPath
    {
        #region Init

        private static readonly object _syncLock = new object();
        private static Func<string> _baseDirectoryFactory = () => AppDomain.CurrentDomain.BaseDirectory;

        private static string GetFolder(string folderName, params string[] preFolders)
        {
            var paths = new string[preFolders.Length + 2];
            paths[0] = _baseDirectoryFactory();
            if (preFolders.Length > 0)
                Array.Copy(preFolders, 0, paths, 1, preFolders.Length);
            paths[paths.Length - 1] = folderName;

            var folder = Path.Combine(paths);

            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            return folder;
        }

        #endregion Init

        public static void SetBaseDir(Func<string> setDir)
        {
            if (setDir == null) throw new ArgumentNullException(nameof(setDir));
            lock (_syncLock)
            {
                _baseDirectoryFactory = setDir;
            }
        }

        public static string BaseDirectory => _baseDirectoryFactory();

        public static string LogFolder(string name = "Logs", params string[] preFolders) => GetFolder(name, preFolders);

        public static string DatabaseFolder(string name = "Databases", params string[] preFolders) => GetFolder(name, preFolders);

        public static string ResourceFolder(string name = "Resources", params string[] preFolders) => GetFolder(name, preFolders);

        public static string ReportFolder(string name = "Reports", params string[] preFolders) => GetFolder(name, preFolders);

        public static string OutputFolder(string name = "Outputs", params string[] preFolders) => GetFolder(name, preFolders);

        public static string SettingFolder(string name = "Settings", params string[] preFolders) => GetFolder(name, preFolders);

        public static string ImageFolder(string name = "Images", params string[] preFolders) => GetFolder(name, preFolders);
    }
}