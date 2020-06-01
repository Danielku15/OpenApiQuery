using System;
using System.Linq.Expressions;

namespace OpenApiQuery.Utils
{
    public static class ExpressionHelper
    {
        /// <summary>
        /// Converts (f : A => B, g: B => C) into (A => C), inlining f body into g
        /// </summary>
        public static Expression<Func<TSource, TResult>> Compose<TSource, TIntermediate, TResult>(
            Expression<Func<TSource, TIntermediate>> first,
            Expression<Func<TIntermediate, TResult>> second)
        {
            var param = Expression.Parameter(typeof(TSource), first.Parameters[0].Name);
            var intermediateValue = ReplaceParameter(first.Body, first.Parameters[0], param);
            var body = ReplaceParameter(second.Body, second.Parameters[0], intermediateValue);
            return Expression.Lambda<Func<TSource, TResult>>(body, param);
        }

        private static Expression ReplaceParameter(Expression expression,
            ParameterExpression toReplace,
            Expression newExpression) =>
            new ParameterReplaceVisitor(toReplace, newExpression)
                .Visit(expression);

        private class ParameterReplaceVisitor : ExpressionVisitor
        {
            private readonly ParameterExpression _from;
            private readonly Expression _to;
            public ParameterReplaceVisitor(ParameterExpression from, Expression to)
            {
                _from = from;
                _to = to;
            }
            protected override Expression VisitParameter(ParameterExpression node)
            {
                return node == _from ? _to : node;
            }
        }
    }
}
