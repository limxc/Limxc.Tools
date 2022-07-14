using System.Threading.Tasks;
using Limxc.Tools.Contract.Enums;

namespace Limxc.Tools.Contract.Interfaces
{
    public interface IReportService
    {
        Task<byte[]> PrintAsync<T>(T obj, string frxFullPath, ReportMode mode = ReportMode.Show,
            string exportFilePath = null);

        byte[] Print<T>(T obj, string frxFullPath, ReportMode mode = ReportMode.Show,
            string exportFilePath = null);
    }
}