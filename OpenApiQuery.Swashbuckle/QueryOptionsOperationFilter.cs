using System.Collections.Generic;
using System.Linq;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace OpenApiQuery.Swashbuckle
{
    internal class QueryOptionsOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var queryOptions = operation.Parameters.FirstOrDefault(p =>
                SwashbuckleHelpers.IsSchemaOpenApiQueryType(p.Schema, context, "query-options"));
            if (queryOptions != null)
            {
                operation.Parameters.Remove(queryOptions);
                AddQueryOptions(operation);
            }
        }

        private void AddQueryOptions(OpenApiOperation operation)
        {
            // add filters
            if (operation.Parameters == null)
            {
                operation.Parameters = new List<OpenApiParameter>();
            }

            operation.Parameters.Add(new OpenApiParameter
            {
                Name = QueryOptionKeys.CountKeys.First(),
                In = ParameterLocation.Query,
                Schema = new OpenApiSchema
                {
                    Type = "boolean"
                },
                Description = "Whether to include the total number of items in the result set before paging",
            });
            operation.Parameters.Add(new OpenApiParameter
            {
                Name = QueryOptionKeys.ExpandKeys.First(),
                In = ParameterLocation.Query,
                Schema = new OpenApiSchema
                {
                    Type = "string"
                },
                Description = "Which navigation properties to include in the result set"
            });
            operation.Parameters.Add(new OpenApiParameter
            {
                Name = QueryOptionKeys.FilterKeys.First(),
                In = ParameterLocation.Query,
                Schema = new OpenApiSchema
                {
                    Type = "string"
                },
                Description = "A filter to select only a subset of the overall results"
            });
            operation.Parameters.Add(new OpenApiParameter
            {
                Name = QueryOptionKeys.OrderbyKeys.First(),
                In = ParameterLocation.Query,
                Schema = new OpenApiSchema
                {
                    Type = "string"
                },
                Description = "A comma separated list of properties to sort the result set"
            });
            operation.Parameters.Add(new OpenApiParameter
            {
                Name = QueryOptionKeys.SelectKeys.First(),
                In = ParameterLocation.Query,
                Schema = new OpenApiSchema
                {
                    Type = "string"
                },
                Description = "Limit the properties that are included in the result"
            });
            operation.Parameters.Add(new OpenApiParameter
            {
                Name = QueryOptionKeys.SkipKeys.First(),
                In = ParameterLocation.Query,
                Schema = new OpenApiSchema
                {
                    Type = "integer"
                },
                Description = "The number of items to skip for paging"
            });
            operation.Parameters.Add(new OpenApiParameter
            {
                Name = QueryOptionKeys.TopKeys.First(),
                In = ParameterLocation.Query,
                Schema = new OpenApiSchema
                {
                    Type = "integer"
                },
                Description = "The number of elements to include from the result set for paging"
            });
        }
    }
}