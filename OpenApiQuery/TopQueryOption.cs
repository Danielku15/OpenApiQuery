using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using OpenApiQuery.Utils;

namespace OpenApiQuery
{
    public class TopQueryOption
    {
        private static readonly MethodInfo TakeInfo =
            ReflectionHelper.GetMethod<IEnumerable<object>, IEnumerable<object>>(x => x.Take(1));

        public string RawValue { get; set; }
        public int? Value { get; set; }

        public IQueryable<T> ApplyTo<T>(IQueryable<T> queryable)
        {
            if (Value != null)
            {
                queryable = queryable.Take(Value.Value);
            }

            return queryable;
        }

        public Expression ApplyTo(Expression expression, Type itemType)
        {
            if (Value != null)
            {
                expression = Expression.Call(null, TakeInfo.MakeGenericMethod(itemType),
                    expression,
                    Expression.Constant(Value.Value)
                );
            }

            return expression;
        }


        public void Initialize(HttpContext httpContext, ModelStateDictionary modelState)
        {
            if (httpContext.Request.Query.TryGetValues(QueryOptionKeys.TopKeys, out var values))
            {
                using var enumerator = values.GetEnumerator();
                if (enumerator.MoveNext())
                {
                    Initialize(enumerator.Current, modelState);

                    if (enumerator.MoveNext())
                    {
                        modelState.TryAddModelError(QueryOptionKeys.TopKeys.First(),
                            "Multiple top clauses found, only one can be specified.");
                    }
                }
            }
        }

        public void Initialize(string value, ModelStateDictionary modelState)
        {
            if (int.TryParse(value, out var x))
            {
                RawValue = value;
                Value = x;
            }
            else
            {
                modelState.TryAddModelError(QueryOptionKeys.TopKeys.First(),
                    "Invalid value specified for top, must be number.");
            }
        }
    }
}
