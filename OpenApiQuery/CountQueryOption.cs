using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace OpenApiQuery
{
    public class CountQueryOption
    {
        public string RawValue { get; private set; }
        public bool? Value { get; set; }

        public void Initialize(HttpContext httpContext, ModelStateDictionary modelState)
        {
            if (httpContext.Request.Query.TryGetValue("$count", out var values))
            {
                if (values.Count == 1)
                {
                    if (bool.TryParse(values[0], out var x))
                    {
                        RawValue = values[0];
                        Value = x;
                    }
                    else
                    {
                        modelState.TryAddModelError("$count",
                            "Invalid value specified for $count, must be bool.");
                    }
                }
                else
                {
                    modelState.TryAddModelError("$count",
                        "Multiple top clauses found, only one can be specified.");
                }
            }
        }
    }
}