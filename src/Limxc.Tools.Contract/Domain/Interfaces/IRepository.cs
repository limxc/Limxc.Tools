using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Limxc.Tools.Contract.Domain.Common;

namespace Limxc.Tools.Contract.Domain.Interfaces
{
    public interface IRepository<in TId, T> : IReadRepository<TId, T> where T : BaseEntity<TId>, IAggregateRoot
    {
        T Save(T entity);
        bool SaveChanges(IEnumerable<T> entities);
        int Delete(Expression<Func<T, bool>> predicate);
        bool DeleteById(TId id);
        bool Delete(T entity);
    }

    public interface IRepository<T> : IRepository<Guid, T>
        where T : BaseEntity<Guid>, IAggregateRoot
    {
    }
}