using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace OpenApiQuery
{
    public class OrderByClause
    {
        public OrderByDirection Direction { get; }
        public Expression Expression { get; }

        public OrderByClause(Expression expression, OrderByDirection direction)
        {
            Direction = direction;
            Expression = expression;
        }

        public IQueryable<T> ApplyTo<T>(IQueryable<T> queryable, ParameterExpression parameter, bool isFirstClause)
        {
            var orderKeyType = Expression.Type;
            // ApplyTo<T, orderKeyType>(queryable, parameter)

            // TODO: caching
            var method = ApplyToInfo.MakeGenericMethod(typeof(T), orderKeyType);
            return (IQueryable<T>) method.Invoke(this, new object[] {queryable, parameter, isFirstClause});
        }

        private static readonly MethodInfo ApplyToInfo =
            typeof(OrderByClause).GetMethod(nameof(ApplyToWithKeyType), BindingFlags.Instance | BindingFlags.NonPublic);

        private IQueryable<T> ApplyToWithKeyType<T, TKey>(IQueryable<T> queryable, ParameterExpression parameter,
            bool isFirstClause)
        {
            var lambda = Expression.Lambda<Func<T, TKey>>(
                Expression,
                parameter
            );

            if (!isFirstClause && queryable is IOrderedQueryable<T> orderedQueryable)
            {
                if (Direction == OrderByDirection.Acending)
                {
                    return orderedQueryable.ThenBy(lambda);
                }

                return orderedQueryable.ThenByDescending(lambda);
            }

            if (Direction == OrderByDirection.Acending)
            {
                return queryable.OrderBy(lambda);
            }

            return queryable.OrderByDescending(lambda);
        }
    }
}