using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Limxc.Tools.Core.Common
{
    public class EnvPath
    {
        private static readonly Lazy<EnvPath> Lazy = new(() => new EnvPath());
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
        ///     BaseDirectory/preFolders/Logs
        /// </summary>
        /// <param name="folderName"></param>
        /// <param name="preFolders"></param>
        /// <returns></returns>
        public string LogFolder(string folderName = "Logs", params string[] preFolders)
        {
            return Folder(folderName, preFolders);
        }

        /// <summary>
        ///     BaseDirectory/preFolders/Databases
        /// </summary>
        /// <param name="folderName"></param>
        /// <param name="preFolders"></param>
        /// <returns></returns>
        public string DatabaseFolder(string folderName = "Databases", params string[] preFolders)
        {
            return Folder(folderName, preFolders);
        }

        /// <summary>
        ///     BaseDirectory/preFolders/Resources
        /// </summary>
        /// <param name="folderName"></param>
        /// <param name="preFolders"></param>
        /// <returns></returns>
        public string ResourceFolder(string folderName = "Resources", params string[] preFolders)
        {
            return Folder(folderName, preFolders);
        }

        /// <summary>
        ///     BaseDirectory/preFolders/Reports
        /// </summary>
        /// <param name="folderName"></param>
        /// <param name="preFolders"></param>
        /// <returns></returns>
        public string ReportFolder(string folderName = "Reports", params string[] preFolders)
        {
            return Folder(folderName, preFolders);
        }

        /// <summary>
        ///     BaseDirectory/preFolders/Outputs
        /// </summary>
        /// <param name="folderName"></param>
        /// <param name="preFolders"></param>
        /// <returns></returns>
        public string OutputFolder(string folderName = "Outputs", params string[] preFolders)
        {
            return Folder(folderName, preFolders);
        }

        /// <summary>
        ///     BaseDirectory/preFolders/Settings
        /// </summary>
        /// <param name="folderName"></param>
        /// <param name="preFolders"></param>
        /// <returns></returns>
        public string SettingFolder(string folderName = "Settings", params string[] preFolders)
        {
            return Folder(folderName, preFolders);
        }

        /// <summary>
        ///     BaseDirectory/preFolders/Images
        /// </summary>
        /// <param name="folderName"></param>
        /// <param name="preFolders"></param>
        /// <returns></returns>
        public string ImageFolder(string folderName = "Images", params string[] preFolders)
        {
            return Folder(folderName, preFolders);
        }

        /// <summary>
        ///     BaseDirectory/preFolders/folderName
        /// </summary>
        /// <param name="folderName"></param>
        /// <param name="preFolders"></param>
        /// <returns></returns>
        public string Folder(string folderName, params string[] preFolders)
        {
            return PathCreator(preFolders.Append(folderName).ToArray());
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
        /// <param name="folders"></param>
        /// <returns></returns>
        private string PathCreator(params string[] folders)
        {
            var list = new List<string> { _baseDirectoryFactory() };
            list.AddRange(folders);

            var folder = Path.Combine(list.Where(p => p != null).ToArray());
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            return folder;
        }
    }
}