using System;
using System.Linq.Expressions;

namespace Limxc.Tools.Specification
{
    public static class SpecExtension
    {
        public static ISpec<T> And<T>(this ISpec<T> left, ISpec<T> right)
        {
            return new Spec<T>(left.Expression.And(right.Expression));
        }

        public static ISpec<T> Or<T>(this ISpec<T> left, ISpec<T> right)
        {
            return new Spec<T>(left.Expression.Or(right.Expression));
        }

        public static ISpec<T> Not<T>(this ISpec<T> one)
        {
            return new Spec<T>(one.Expression.Not());
        }

        public static Expression<Func<T, bool>> Not<T>(this Expression<Func<T, bool>> one)
        {
            var candidateExpr = one.Parameters[0];
            var body = Expression.Not(one.Body);

            return Expression.Lambda<Func<T, bool>>(body, candidateExpr);
        }

        public static Expression<Func<T, bool>> And<T>(
            this Expression<Func<T, bool>> one,
            Expression<Func<T, bool>> another
        )
        {
            var candidateExpr = Expression.Parameter(typeof(T), "candidate");
            var parameterReplacer = new ParameterReplacer(candidateExpr);

            var left = parameterReplacer.Replace(one.Body);
            var right = parameterReplacer.Replace(another.Body);

            var body = Expression.And(left, right);

            return Expression.Lambda<Func<T, bool>>(body, candidateExpr);
        }

        public static Expression<Func<T, bool>> Or<T>(
            this Expression<Func<T, bool>> one,
            Expression<Func<T, bool>> another
        )
        {
            var candidateExpr = Expression.Parameter(typeof(T), "candidate");
            var parameterReplacer = new ParameterReplacer(candidateExpr);

            var left = parameterReplacer.Replace(one.Body);
            var right = parameterReplacer.Replace(another.Body);
            var body = Expression.Or(left, right);

            return Expression.Lambda<Func<T, bool>>(body, candidateExpr);
        }

        internal class ParameterReplacer : ExpressionVisitor
        {
            public ParameterReplacer(ParameterExpression paramExpr)
            {
                ParameterExpression = paramExpr;
            }

            public ParameterExpression ParameterExpression { get; }

            public Expression Replace(Expression expr)
            {
                return Visit(expr);
            }

            protected override Expression VisitParameter(ParameterExpression p)
            {
                return ParameterExpression;
            }
        }
    }
}