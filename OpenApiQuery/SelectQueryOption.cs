using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace OpenApiQuery
{
    public class SelectQueryOption
    {
        public OpenApiQueryOptions QueryOptions { get; }

        public SelectQueryOption(OpenApiQueryOptions queryOptions)
        {
            QueryOptions = queryOptions;
        }

        public string RawValue { get; set; }
        public IList<SelectClause> Clauses { get; set; }

        public void Initialize(HttpContext httpContext, ILogger<OpenApiQueryOptions> logger)
        {
            
        }
    }
}