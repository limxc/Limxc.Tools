using System.Threading.Tasks;

namespace Limxc.Tools.Contract.Interfaces
{
    public interface IReportService
    {
        Task<byte[]> PrintAsync<T>(T obj, string frxFullPath, ReportMode mode = ReportMode.Show,
            string exportFilePath = null);

        byte[] Print<T>(T obj, string frxFullPath, ReportMode mode = ReportMode.Show,
            string exportFilePath = null);
    }

    public enum ReportMode
    {
        Design,
        Show,
        Print
    }
}