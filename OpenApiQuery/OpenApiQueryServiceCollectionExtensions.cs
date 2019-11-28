using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using OpenApiQuery.Parsing;
using OpenApiQuery.Serialization;

namespace OpenApiQuery
{
    public static class OpenApiQueryServiceCollectionExtensions
    {
        public static IMvcCoreBuilder AddOpenApiQuery(this IMvcCoreBuilder builder)
        {
            var services = builder.Services;

            services.TryAddEnumerable(ServiceDescriptor
                .Transient<IConfigureOptions<JsonOptions>, OpenApiOptionsSetup>());
            services.AddSingleton<IOpenApiTypeHandler, DefaultOpenApiTypeHandler>();

            return builder;
        }
    }
}
