using System.Threading.Tasks;

namespace Limxc.Tools.Core.Abstractions
{
    public interface IReportService
    {
        Task<byte[]> PrintAsync<T>(T obj, string frxFullPath, ReportMode mode = ReportMode.Design,
            string exportFilePath = null);
    }

    public enum ReportMode
    {
        Design,
        Show,
        Print
    }
}