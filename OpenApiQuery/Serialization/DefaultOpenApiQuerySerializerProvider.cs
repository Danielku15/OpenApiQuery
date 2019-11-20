using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace OpenApiQuery.Serialization
{
    public class DefaultOpenApiQuerySerializerProvider : IOpenApiQuerySerializerProvider
    {
        private IOpenApiQuerySerializer _instance;

        public IOpenApiQuerySerializer
            GetSerializerForResult<T>(HttpContext context, OpenApiQueryResult result,
                OpenApiQueryApplyResult<T> appliedQueryResult)
        {
            return _instance ??= ActivatorUtilities.CreateInstance<SystemTextOpenApiQuerySerializer>(context.RequestServices);
        }
    }
}