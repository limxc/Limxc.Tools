using System.Threading.Tasks;
using Limxc.Tools.Contract.Dtos;

namespace Limxc.Tools.Contract.Interfaces
{
    public interface IReportService
    {
        Task<byte[]> PrintAsync<T>(ReportDto<T> dto) where T : class;

        byte[] Print<T>(ReportDto<T> dto) where T : class;
    }
}