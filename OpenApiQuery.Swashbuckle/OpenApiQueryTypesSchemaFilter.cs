using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace OpenApiQuery.Swashbuckle
{
    internal class OpenApiQueryTypesSchemaFilter : ISchemaFilter
    {
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            if (context.Type.IsGenericType)
            {
                if (context.Type.GetGenericTypeDefinition() == typeof(OpenApiQueryOptions<>))
                {
                    schema.Extensions["oaq-type"] = new OpenApiString("query-options");
                }
                else if (context.Type.GetGenericTypeDefinition() == typeof(Delta<>))
                {
                    schema.Extensions["oaq-type"] = new OpenApiString("delta");
                    if (context.SchemaRepository.TryGetIdFor(context.Type.GetGenericArguments()[0], out var schemaId))
                    {
                        schema.Extensions["oaq-ref"] = new OpenApiString(schemaId);
                    }
                }
                else if (context.Type.GetGenericTypeDefinition() == typeof(Single<>))
                {
                    schema.Extensions["oaq-type"] = new OpenApiString("single");
                    if (context.SchemaRepository.TryGetIdFor(context.Type.GetGenericArguments()[0], out var schemaId))
                    {
                        schema.Extensions["oaq-ref"] = new OpenApiString(schemaId);
                    }
                }
            }
        }
    }
}