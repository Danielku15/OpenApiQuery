using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace OpenApiQuery.Swashbuckle
{
    public static class OpenApiQueryServiceCollectionExtensions
    {
        public static IMvcCoreBuilder AddOpenApiQuerySwashbuckle(this IMvcCoreBuilder builder)
        {
            var services = builder.Services;

            services.TryAddEnumerable(ServiceDescriptor
                .Transient<IConfigureOptions<SwaggerGenOptions>, SwaggerGenOptionsSetup>());

            return builder;
        }
    }
}
