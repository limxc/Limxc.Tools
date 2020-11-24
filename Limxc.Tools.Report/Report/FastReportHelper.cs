using Limxc.Tools.Utils;
using FastReport;
using System;
using System.Collections;
using System.IO;
using System.Threading.Tasks;

namespace Limxc.Tools.Report
{
    public enum ReportOptionMode
    {
        Design,
        Show,
        Print
    }

    public class ReportService  
    {
        private string _reportFolder() => Path.Combine(EnvPath.BaseDirectory, "Reports");

        private string _defReportPath() => Path.Combine(EnvPath.BaseDirectory, "Reports", "Default.frx");

        /// <summary>
        /// 打印报告单,并保存pdf及png
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="frxName"></param>
        /// <param name="outputFilePath">null不保存; 无后缀,自动存储为png及pdf</param>
        public void Print<T>(T obj, string frxName, ReportOptionMode mode = ReportOptionMode.Design, string outputFilePath = null) where T : IEnumerable
        {
            #region 汉化

            var file = Path.Combine(_reportFolder(), @"Chinese.frl");

            if (File.Exists(file))
            {
                FastReport.Utils.Res.LocaleFolder = _reportFolder();
                FastReport.Utils.Res.LoadLocale(file);
            }

            #endregion 汉化

            #region File

            if (!frxName.EndsWith(".frx"))
                frxName += ".frx";

            var frxPath = Path.Combine(_reportFolder(), frxName);

            if (!File.Exists(frxPath))
            {
                File.Copy(_defReportPath(), frxPath);

            }

            #endregion File

            var report = new FastReport.Report();
            try
            {
                //加载模板
                report.Load(frxPath);

                //注册数据
                report.RegisterData(obj, "Data");
                report.Prepare();

                //呈现模式
                if (mode == ReportOptionMode.Design)
                    report.Design();
                else if (mode == ReportOptionMode.Show)
                    report.Show();
                else if (mode == ReportOptionMode.Print)
                    report.PrintPrepared();

                //导出PDF到指定路径
                if (!string.IsNullOrWhiteSpace(outputFilePath))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(outputFilePath));
                    var pdfTask = Task.Run(() =>
                    {
                        using (var pdfExport = new FastReport.Export.Pdf.PDFExport())
                        {
                            pdfExport.Compressed = true;
                            report.Export(pdfExport, outputFilePath + ".pdf");
                        }
                    });
                    var pngTask = Task.Run(() =>
                    {
                        using (var imgExport = new FastReport.Export.Image.ImageExport())
                        {
                            imgExport.JpegQuality = 90;
                            imgExport.ImageFormat = FastReport.Export.Image.ImageExportFormat.Png;
                            imgExport.Resolution = 200;
                            imgExport.SeparateFiles = false;
                            report.Export(imgExport, outputFilePath + ".png");
                        };
                    });
                    Task.WhenAll(pdfTask, pngTask).ContinueWith(_ =>
                    {
                        report?.Dispose();
                    });
                }
            }
            catch (Exception)
            {
                report?.Dispose();
                throw;
            }
        }

        /// <summary>
        /// 获取Pdf文件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="frxName"></param>
        public Task<byte[]> GetPdf<T>(T obj, string frxName) where T : IEnumerable
        {
            return Task.Run(() =>
            {
                if (!frxName.EndsWith(".frx"))
                    frxName += ".frx";
                var frxPath = Path.Combine(_reportFolder(), frxName);

                if (!File.Exists(frxPath))
                {
                    return new byte[] { };
                }

                var report = new FastReport.Report();
                //加载模板
                report.Load(frxPath);

                //注册数据
                report.RegisterData(obj, "Data");
                report.Prepare();

                using (var pdfExport = new FastReport.Export.Pdf.PDFExport())
                {
                    pdfExport.Compressed = true;
                    using (var ms = new MemoryStream())
                    {
                        report.Export(pdfExport, ms);
                        return ms.ToArray();
                    }
                }
            });
        }
    }
}
