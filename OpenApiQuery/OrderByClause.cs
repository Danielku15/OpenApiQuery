using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using OpenApiQuery.Utils;

namespace OpenApiQuery
{
    public class OrderByClause
    {
        private static readonly MethodInfo OrderByInfo =
            ReflectionHelper.GetMethod<IEnumerable<object>, IEnumerable<object>>(x => x.OrderBy(y => y == null));

        private static readonly MethodInfo OrderByDescendingInfo =
            ReflectionHelper.GetMethod<IEnumerable<object>, IEnumerable<object>>(x =>
                x.OrderByDescending(y => y == null));

        private static readonly MethodInfo ThenByInfo =
            ReflectionHelper.GetMethod<IOrderedEnumerable<object>, IEnumerable<object>>(x => x.ThenBy(y => y == null));

        private static readonly MethodInfo ThenByDescendingInfo =
            ReflectionHelper.GetMethod<IOrderedEnumerable<object>, IEnumerable<object>>(x =>
                x.ThenByDescending(y => y == null));

        private static readonly MethodInfo ApplyToInfo =
            typeof(OrderByClause).GetMethod(nameof(ApplyToWithKeyType), BindingFlags.Instance | BindingFlags.NonPublic);

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

        public Expression ApplyTo(Expression expression, ParameterExpression parameter, bool isFirstClause)
        {
            var orderKeyType = Expression.Type;
            var itemType = parameter.Type;

            MethodInfo sortMethod;

            if (!isFirstClause)
            {
                sortMethod = Direction == OrderByDirection.Acending ? ThenByInfo : ThenByDescendingInfo;
            }
            else
            {
                sortMethod = Direction == OrderByDirection.Acending ? OrderByInfo : OrderByDescendingInfo;
            }

            sortMethod = sortMethod.MakeGenericMethod(itemType, orderKeyType);

            var funcType = typeof(Func<,>).MakeGenericType(itemType, orderKeyType);
            var sortLambda = Expression.Lambda(
                funcType,
                Expression,
                parameter
            );

            expression = Expression.Call(null, sortMethod,
                expression,
                sortLambda
            );

            return expression;
        }

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
