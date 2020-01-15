using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace OpenApiQuery.Swashbuckle
{
    internal class SwaggerGenOptionsSetup : IConfigureOptions<SwaggerGenOptions>
    {
        public void Configure(SwaggerGenOptions options)
        {
            options.OperationFilter<QueryOptionsOperationFilter>();
            options.OperationFilter<SingleDeltaOperationFilter>();
            options.SchemaFilter<OpenApiQueryTypesSchemaFilter>();
        }
    }
}
