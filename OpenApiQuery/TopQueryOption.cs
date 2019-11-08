using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace OpenApiQuery
{
    public class TopQueryOption
    {
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

        public void Initialize(HttpContext httpContext, ModelStateDictionary modelState)
        {
            if (httpContext.Request.Query.TryGetValue("$top", out var values))
            {
                if (values.Count == 1)
                {
                    if (int.TryParse(values[0], out var x))
                    {
                        RawValue = values[0];
                        Value = x;
                    }
                    else
                    {
                        modelState.TryAddModelError("$top",
                            "Invalid value specified for $top, must be number.");
                    }
                }
                else
                {
                    modelState.TryAddModelError("$top",
                        "Multiple top clauses found, only one can be specified.");
                }
            }
        }
    }
}