using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using FastReport.Export.Image;
using FastReport.Export.Pdf;
using FastReport.Utils;
using Limxc.Tools.Core.Abstractions;

namespace Limxc.Tools.Fx.Report
{
    public class ReportService : IReportService
    {
        /// <summary>
        ///     打印报告单,保存pdf及png,返回Pdf的Byte数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="frxFullPath"></param>
        /// <param name="mode"></param>
        /// <param name="exportFilePath">null不保存; 后缀可无,自动存储为png及pdf</param>
        public async Task<byte[]> PrintAsync<T>(T obj, string frxFullPath, ReportMode mode = ReportMode.Design,
            string exportFilePath = null)
        {
            var folder = Path.GetDirectoryName(frxFullPath);
            if (!Directory.Exists(folder))
                throw new DirectoryNotFoundException($"Directory not Found. {folder}");


            #region 汉化

            var file = Path.Combine(folder, "Chinese.frl");

            if (File.Exists(file))
            {
                Res.LocaleFolder = folder;
                Res.LoadLocale(file);
            }

            #endregion 汉化

            #region FrxFile

            if (!frxFullPath.ToLower().EndsWith(".frx"))
                frxFullPath += ".frx";

            if (!File.Exists(frxFullPath))
                throw new FileNotFoundException($"File Not Found. {frxFullPath}");

            #endregion

            var report = new FastReport.Report();

            try
            {
                //加载模板
                report.Load(frxFullPath);

                //注册数据
                if (obj is IEnumerable enumerableObj)
                    report.RegisterData(enumerableObj, "Data");
                else
                    report.RegisterData(new[] {obj}, "Data");

                report.Prepare(false);

                //var exportTask = ExportAsync(report, exportFilePath);
                //呈现模式
                if (mode == ReportMode.Design)
                    //await exportTask;
                    report.Design();
                else if (mode == ReportMode.Show)
                    //await exportTask;
                    report.Show();
                else if (mode == ReportMode.Print)
                    report.PrintPrepared();
                //await exportTask;

                return await ExportAsync(report, exportFilePath);
            }
            finally
            {
                report.Dispose();
            }
        }

        private async Task<byte[]> ExportAsync(FastReport.Report report, string exportFilePath)
        {
            var data = new byte[0];
            var tasks = new List<Task>();
            //导出PDF到指定路径

            var folder = Path.GetDirectoryName(exportFilePath);
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder ?? throw new InvalidOperationException());

            var fileName = Path.GetFileNameWithoutExtension(exportFilePath);
            if (!string.IsNullOrWhiteSpace(folder) && !string.IsNullOrWhiteSpace(fileName))
            {
                var pdfTask = Task.Run(() =>
                {
                    using (var pdfExport = new PDFExport())
                    {
                        pdfExport.Compressed = true;
                        report.Export(pdfExport, Path.Combine(folder, fileName + ".pdf"));
                        using (var ms = new MemoryStream())
                        {
                            report.Export(pdfExport, ms);
                            data = ms.ToArray();
                        }
                    }
                });
                tasks.Add(pdfTask);
                var pngTask = Task.Run(() =>
                {
                    using (var imgExport = new ImageExport())
                    {
                        imgExport.JpegQuality = 90;
                        imgExport.ImageFormat = ImageExportFormat.Png;
                        imgExport.Resolution = 200;
                        imgExport.SeparateFiles = false;
                        report.Export(imgExport, Path.Combine(folder, fileName + ".png"));
                    }
                });
                tasks.Add(pngTask);
            }
            else
            {
                var byteTask = Task.Run(() =>
                {
                    using (var pdfExport = new PDFExport())
                    {
                        pdfExport.Compressed = true;
                    }
                });
                tasks.Add(byteTask);
            }

            await Task.WhenAll(tasks);
            return data;
        }
    }
}