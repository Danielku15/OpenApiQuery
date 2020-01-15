using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace OpenApiQuery.Swashbuckle
{
    internal class SwashbuckleHelpers
    {
        public static string GetSchemaOpenApiQueryType(OpenApiSchema schema, OperationFilterContext context)
        {
            var type = GetSchemaOpenApiQueryType(schema);
            if (type != null)
            {
                return type;
            }

            if (schema.Reference != null
                && context.SchemaRepository.Schemas.TryGetValue(schema.Reference.Id, out var reference))
            {
                type = GetSchemaOpenApiQueryType(reference, context);
            }

            return type;
        }

        private static string GetSchemaOpenApiQueryType(OpenApiSchema schema)
        {
            if (schema.Extensions.TryGetValue("oaq-type", out var value) && value is OpenApiString s)
            {
                return s.Value;
            }

            return null;
        }


        public static bool IsSchemaOpenApiQueryType(
            OpenApiSchema schema,
            OperationFilterContext context,
            string type)
        {
            return GetSchemaOpenApiQueryType(schema, context) == type;
        }
    }
}