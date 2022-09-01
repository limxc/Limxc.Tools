using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Limxc.Tools.Contract.Domain.Common;

namespace Limxc.Tools.Contract.Domain.Interfaces
{
    public interface IReadRepository<in TId, T> where T : BaseEntity<TId>, IAggregateRoot
    {
        T FindById(TId id);
        IList<T> Find(Expression<Func<T, bool>> predicate);

        /// <summary>
        ///     分页查询
        /// </summary>
        /// <param name="predicate"></param>
        /// <param name="descendingBy"></param>
        /// <param name="pageNum">1~n</param>
        /// <param name="pageSize">1~n</param>
        /// <param name="pageCount">0~n</param>
        /// <returns></returns>
        IList<T> PageFind<TK>(Expression<Func<T, bool>> predicate, Expression<Func<T, TK>> descendingBy, int pageNum,
            int pageSize, out int pageCount);

        int Count(Expression<Func<T, bool>> predicate);
    }

    public interface IReadRepository<T> : IReadRepository<Guid, T> where T : BaseEntity<Guid>, IAggregateRoot
    {
    }
}