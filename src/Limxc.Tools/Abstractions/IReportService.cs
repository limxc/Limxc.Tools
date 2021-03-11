using System.Collections;
using System.Threading.Tasks;

namespace Limxc.Tools.Abstractions
{
    public interface IReportService
    {
        Task<byte[]> GetPdf<T>(T obj, string frxName) where T : IEnumerable;

        void Print<T>(T obj, string frxName, ReportOptionMode mode = ReportOptionMode.Design,
            string outputFilePath = null) where T : IEnumerable;
    }

    public enum ReportOptionMode
    {
        Design,
        Show,
        Print
    }
}