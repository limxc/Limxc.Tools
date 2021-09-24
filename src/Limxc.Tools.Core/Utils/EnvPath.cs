using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Limxc.Tools.Core.Utils
{
    public class EnvPath
    {
        private static readonly Lazy<EnvPath> Lazy = new Lazy<EnvPath>(() => new EnvPath());
        private Func<string> _baseDirectoryFactory;

        private EnvPath()
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
            _baseDirectoryFactory = () => AppDomain.CurrentDomain.BaseDirectory;
        }

        public static EnvPath Default => Lazy.Value;

        /// <summary>
        ///     default: AppDomain.CurrentDomain.BaseDirectory
        /// </summary>
        public string BaseDirectory => _baseDirectoryFactory();

        /// <summary>
        ///     BaseDirectory/Databases
        /// </summary>
        /// <param name="name"></param>
        /// <param name="paths"></param>
        /// <returns></returns>
        public string DatabaseFolder(string name = "Databases", params string[] paths)
        {
            return Folder(name, paths);
        }

        /// <summary>
        ///     BaseDirectory/Resources
        /// </summary>
        /// <param name="name"></param>
        /// <param name="paths"></param>
        /// <returns></returns>
        public string ResourceFolder(string name = "Resources", params string[] paths)
        {
            return Folder(name, paths);
        }

        /// <summary>
        ///     BaseDirectory/Reports
        /// </summary>
        /// <param name="name"></param>
        /// <param name="paths"></param>
        /// <returns></returns>
        public string ReportFolder(string name = "Reports", params string[] paths)
        {
            return Folder(name, paths);
        }

        /// <summary>
        ///     BaseDirectory/Outputs
        /// </summary>
        /// <param name="name"></param>
        /// <param name="paths"></param>
        /// <returns></returns>
        public string OutputFolder(string name = "Outputs", params string[] paths)
        {
            return Folder(name, paths);
        }

        /// <summary>
        ///     BaseDirectory/Settings
        /// </summary>
        /// <param name="name"></param>
        /// <param name="paths"></param>
        /// <returns></returns>
        public string SettingFolder(string name = "Settings", params string[] paths)
        {
            return Folder(name, paths);
        }

        /// <summary>
        ///     BaseDirectory/Images
        /// </summary>
        /// <param name="name"></param>
        /// <param name="paths"></param>
        /// <returns></returns>
        public string ImageFolder(string name = "Images", params string[] paths)
        {
            return Folder(name, paths);
        }

        /// <summary>
        ///     BaseDirectory/Images
        /// </summary>
        /// <param name="name"></param>
        /// <param name="paths"></param>
        /// <returns></returns>
        public string Folder(string name, params string[] paths)
        {
            return PathCreator(null, paths.Append(name).ToArray());
        }

        /// <summary>
        ///     设置默认文件夹
        /// </summary>
        /// <param name="baseDirectoryFactory"></param>
        public void UseBaseDirectoryFactory(Func<string> baseDirectoryFactory)
        {
            _baseDirectoryFactory = baseDirectoryFactory;
        }

        /// <summary>
        ///     创建路径
        /// </summary>
        /// <param name="baseFolder"></param>
        /// <param name="paths"></param>
        /// <returns></returns>
        private string PathCreator(string baseFolder, params string[] paths)
        {
            if (!Directory.Exists(baseFolder))
                baseFolder = _baseDirectoryFactory();

            var list = new List<string> { baseFolder };
            list.AddRange(paths);

            var folder = Path.Combine(list.Where(p => p != null).ToArray());
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            return folder;
        }
    }
}