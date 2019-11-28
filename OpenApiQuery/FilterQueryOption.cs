using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenApiQuery.Parsing;
using OpenApiQuery.Utils;

namespace OpenApiQuery
{
    public class FilterQueryOption
    {
        private static readonly MethodInfo WhereInfo =
            ReflectionHelper.GetMethod<IEnumerable<object>, IEnumerable<object>>(x => x.Where(y => y == null));

        public ParameterExpression Parameter { get; set; }

        public FilterQueryOption(Type elementType)
        {
            Parameter = Expression.Parameter(elementType);
        }

        public string RawValue { get; set; }
        public Expression FilterClause { get; set; }

        public IQueryable<T> ApplyTo<T>(IQueryable<T> queryable)
        {
            if (FilterClause != null)
            {
                var lambda = Expression.Lambda<Func<T, bool>>(
                    FilterClause,
                    Parameter
                );
                queryable = queryable.Where(lambda);
            }

            return queryable;
        }
        public Expression ApplyTo(Expression expression)
        {
            if (FilterClause != null)
            {
                var funcType = typeof(Func<,>).MakeGenericType(Parameter.Type, typeof(bool));
                var whereParam = Expression.Lambda(funcType,
                    FilterClause,
                    Parameter
                );

                expression = Expression.Call(null, WhereInfo.MakeGenericMethod(Parameter.Type),
                    expression,
                    whereParam
                );
            }

            return expression;
        }

        public void Initialize(HttpContext httpContext, ILogger<OpenApiQueryOptions> logger, ModelStateDictionary modelStateDictionary)
        {
            if (httpContext.Request.Query.TryGetValue("$filter", out var values))
            {
                if (values.Count == 1)
                {
                    RawValue = values[0];
                    var binder = httpContext.RequestServices.GetRequiredService<IOpenApiTypeHandler>();
                    var parser = new QueryExpressionParser(values[0], binder);
                    try
                    {
                        parser.PushThis(Parameter);
                        FilterClause = parser.CommonExpr();
                        parser.PopThis();
                    }
                    catch (Exception e)
                    {
                        logger.LogError(e, "Failed to parse filter");
                        modelStateDictionary.TryAddModelException("$filter", e);
                    }
                }
                else
                {
                    modelStateDictionary.TryAddModelError("$filter", "Only one $filter can be specified per request");
                }
            }
        }

        internal void Initialize(QueryExpressionParser parser, ModelStateDictionary modelStateDictionary)
        {
            try
            {
                parser.PushThis(Parameter);
                FilterClause = parser.CommonExpr();
                parser.PopThis();
            }
            catch (Exception e)
            {
                modelStateDictionary.TryAddModelException("$filter", e);
            }
        }
    }
}
