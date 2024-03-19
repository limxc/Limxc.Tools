using System;
using System.Linq.Expressions;

namespace Limxc.Tools.Specification
{
    public class Spec<T> : ISpec<T>
    {
        public Spec(Expression<Func<T, bool>> isSatisfiedBy)
        {
            Expression = isSatisfiedBy;
        }

        public Expression<Func<T, bool>> Expression { get; }

        public bool IsSatisfiedBy(T candidate)
        {
            return Expression.Compile().Invoke(candidate);
        }
    }
}