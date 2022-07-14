using Limxc.Tools.Contract.Enums;

namespace Limxc.Tools.Contract.Dtos
{
    public class ReportDto<T> where T : class
    {
        public ReportDto()
        {
        }

        public ReportDto(T body, string templatePath, ReportMode mode, string exportFilePath = "")
        {
            Body = body;
            Mode = mode;
            TemplatePath = templatePath;
            ExportFilePath = exportFilePath;
        }

        public T Body { get; set; }
        public string TemplatePath { get; set; }
        public ReportMode Mode { get; set; }
        public string ExportFilePath { get; set; }
    }
}