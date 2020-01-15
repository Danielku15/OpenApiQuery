using System.Linq;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace OpenApiQuery.Swashbuckle
{
    internal class SingleDeltaOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            RewriteParameters(operation, context);
            RewriteBody(operation, context);
        }

        private void RewriteBody(OpenApiOperation operation, OperationFilterContext context)
        {
            if (operation.RequestBody?.Content == null)
            {
                return;
            }

            foreach (var mediaType in operation.RequestBody.Content.ToArray())
            {
                var type = SwashbuckleHelpers.GetSchemaOpenApiQueryType(mediaType.Value.Schema, context);
                switch (type)
                {
                    case "single":
                        mediaType.Value.Schema = UnwrapOpenApiType(mediaType.Value.Schema, "resultItem", context);
                        break;

                    case "delta":
                        mediaType.Value.Schema = UnwrapOpenApiType(mediaType.Value.Schema, "", context);
                        break;
                    case null:
                        break;
                }
            }
        }

        private void RewriteParameters(OpenApiOperation operation, OperationFilterContext context)
        {
            var singleAndDelta = operation.Parameters.Select(p => new
            {
                p,
                type = SwashbuckleHelpers.GetSchemaOpenApiQueryType(p.Schema, context)
            }).Where(p => p.type != null);

            foreach (var parameter in singleAndDelta)
            {
                switch (parameter.type)
                {
                    case "single":
                        parameter.p.Schema = UnwrapOpenApiType(parameter.p.Schema, "resultItem", context);
                        break;
                    case "delta":
                        parameter.p.Schema = UnwrapOpenApiType(parameter.p.Schema, "", context);
                        break;
                }
            }
        }

        private OpenApiSchema UnwrapOpenApiType(
            OpenApiSchema schema,
            string propertyName,
            OperationFilterContext context)
        {
            if (schema.Extensions.TryGetValue("oaq-ref", out var refName) && refName is OpenApiString s &&
                context.SchemaRepository.Schemas.TryGetValue(s.Value, out var refSchema))
            {
                return refSchema;
            }


            if (schema.Properties.TryGetValue(propertyName, out var propSchema))
            {
                return propSchema;
            }

            if (schema.Reference != null &&
                context.SchemaRepository.Schemas.TryGetValue(schema.Reference.Id, out refSchema))
            {
                return UnwrapOpenApiType(refSchema, propertyName, context);
            }

            return null;
        }
    }
}