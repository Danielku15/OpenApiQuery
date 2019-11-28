using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Metadata;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using OpenApiQuery.Utils;

namespace OpenApiQuery
{
    public class SkipQueryOption
    {
        private static readonly MethodInfo SkipInfo =
            ReflectionHelper.GetMethod<IEnumerable<object>, IEnumerable<object>>(x => x.Skip(1));

        public string RawValue { get; set; }
        public int? Value { get; set; }

        public IQueryable<T> ApplyTo<T>(IQueryable<T> queryable)
        {
            if (Value != null)
            {
                queryable = queryable.Skip(Value.Value);
            }

            return queryable;
        }
        public Expression ApplyTo(Expression expression, Type itemType)
        {
            if (Value != null)
            {
                expression = Expression.Call(null, SkipInfo.MakeGenericMethod(itemType),
                    expression,
                    Expression.Constant(Value.Value)
                );
            }

            return expression;
        }

        public void Initialize(HttpContext httpContext, ModelStateDictionary modelState)
        {
            if (httpContext.Request.Query.TryGetValue("$skip", out var values))
            {
                if (values.Count == 1)
                {
                    Initialize(values[0], modelState);
                }
                else
                {
                    modelState.TryAddModelError("$skip",
                        "Multiple skip clauses found, only one can be specified.");
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
                modelState.TryAddModelError("$skip",
                    "Invalid value specified for $skip, must be number.");
            }
        }
    }
}
