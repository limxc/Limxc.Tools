using System;
using System.Linq.Expressions;

// ReSharper disable PartialTypeWithSinglePart

namespace Limxc.Tools.Contract.Domain.Specification
{
    public partial class Spec<T> : ISpec<T>
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