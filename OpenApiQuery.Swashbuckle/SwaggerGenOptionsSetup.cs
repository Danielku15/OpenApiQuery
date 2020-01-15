using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace OpenApiQuery.Swashbuckle
{
    internal class SwaggerGenOptionsSetup : IConfigureOptions<SwaggerGenOptions>
    {
        public void Configure(SwaggerGenOptions options)
        {
            options.MapType<OpenApiQueryOptions>(()=>new OpenApiSchema
            {

            });
        }
    }
}