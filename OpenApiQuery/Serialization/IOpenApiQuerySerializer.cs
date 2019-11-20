using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace OpenApiQuery.Serialization
{
    public interface IOpenApiQuerySerializer
    {
        Task SerializeResultAsync<T>(Stream writeStream, OpenApiQueryResult queryResult,
            OpenApiQueryApplyResult<T> appliedQueryResult,
            CancellationToken cancellationToken) where T : new();
    }
}