using System;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenApiQuery.Parsing;

namespace OpenApiQuery
{
    public class FilterQueryOption
    {
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

        public void Initialize(HttpContext httpContext, ILogger<OpenApiQueryOptions> logger, ModelStateDictionary modelStateDictionary)
        {
            if (httpContext.Request.Query.TryGetValue("$filter", out var values))
            {
                if (values.Count == 1)
                {
                    RawValue = values[0];
                    var binder = httpContext.RequestServices.GetRequiredService<IOpenApiQueryExpressionBinder>();
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
    }
}