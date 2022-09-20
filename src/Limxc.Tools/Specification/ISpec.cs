using System;
using System.Linq.Expressions;

namespace Limxc.Tools.Specification
{
    public interface ISpec<T>
    {
        Expression<Func<T, bool>> Expression { get; }

        bool IsSatisfiedBy(T candidate);
    }
}