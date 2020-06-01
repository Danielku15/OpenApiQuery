using Microsoft.Extensions.DependencyInjection;
using OpenApiQuery.Parsing;

namespace OpenApiQuery
{
    public static class OpenApiQueryServiceCollectionExtensions
    {
        public static IServiceCollection AddOpenApiQuery(this IServiceCollection services)
        {
            services.AddSingleton<IOpenApiTypeHandler, DefaultOpenApiTypeHandler>();
            return services;
        }
    }
}
