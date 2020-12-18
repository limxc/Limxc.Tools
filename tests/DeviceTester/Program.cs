using DeviceTester.Gmd;
using DeviceTester.Tcf;
using System;
using System.Windows.Forms;

namespace DeviceTester
{
    internal static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            //Application.Run(new FrmSerialPort());
            //Application.Run(new FrmGmd());
            Application.Run(new FrmTcf());
        }
    }
}