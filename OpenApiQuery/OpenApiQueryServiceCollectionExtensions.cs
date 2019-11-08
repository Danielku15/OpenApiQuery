using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using OpenApiQuery.Parsing;

namespace OpenApiQuery
{
    public static class OpenApiQueryServiceCollectionExtensions
    {
        public static IMvcCoreBuilder AddOpenApiQuery(this IMvcCoreBuilder builder)
        {
            var services = builder.Services;
            services.TryAddEnumerable(ServiceDescriptor
                .Transient<IConfigureOptions<MvcOptions>, OpenApiOptionsSetup>());
            services.AddSingleton<IExpressionBinder, DefaultExpressionBinder>();

            return builder;
        }
    }
}