using Microsoft.AspNetCore.Http;

namespace OpenApiQuery.Serialization
{
    public interface IOpenApiQuerySerializerProvider
    {
        IOpenApiQuerySerializer GetSerializerForResult<T>(HttpContext context, OpenApiQueryResult result,
            OpenApiQueryApplyResult<T> appliedQueryResult);
    }
}