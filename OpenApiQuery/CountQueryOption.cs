using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using OpenApiQuery.Utils;

namespace OpenApiQuery
{
    public class CountQueryOption
    {
        public string RawValue { get; private set; }
        public bool? Value { get; set; }

        public void Initialize(HttpContext httpContext, ModelStateDictionary modelState)
        {
            if (httpContext.Request.Query.TryGetValues(QueryOptionKeys.CountKeys, out var values))
            {
                using var enumerator = values.GetEnumerator();
                if (enumerator.MoveNext())
                {
                    if (bool.TryParse(enumerator.Current, out var x))
                    {
                        RawValue = enumerator.Current;
                        Value = x;
                    }
                    else
                    {
                        modelState.TryAddModelError(QueryOptionKeys.CountKeys.First(),
                            "Invalid value specified for count, must be bool.");
                    }

                    if (enumerator.MoveNext())
                    {
                        modelState.TryAddModelError(QueryOptionKeys.CountKeys.First(),
                            "Multiple count clauses found, only one can be specified.");
                    }
                }
            }
        }
    }
}
