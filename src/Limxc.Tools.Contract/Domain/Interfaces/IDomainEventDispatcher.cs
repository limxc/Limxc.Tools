using System.Threading.Tasks;
using Limxc.Tools.Contract.Domain.Common;

namespace Limxc.Tools.Contract.Domain.Interfaces
{
    public interface IDomainEventDispatcher
    {
        Task DispatchAndClearEvents<TId>(params BaseEntity<TId>[] entitiesWithEvents);
    }
}