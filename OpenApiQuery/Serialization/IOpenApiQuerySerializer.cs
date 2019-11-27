using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace OpenApiQuery.Serialization
{
    public class OpenApiSerializationContext
    {
        public OpenApiQueryResult QueryResult { get; }

        public OpenApiSerializationContext(OpenApiQueryResult queryResult)
        {
            QueryResult = queryResult;
        }
    }

    public class OpenApiSerializationContext<T> : OpenApiSerializationContext
    {
        public OpenApiQueryApplyResult<T> ApplyResult { get; }

        public OpenApiSerializationContext(OpenApiQueryResult queryResult,
            OpenApiQueryApplyResult<T> applyResult) : base(queryResult)
        {
            ApplyResult = applyResult;
        }
    }

    public interface IOpenApiQuerySerializer
    {
        Task SerializeResultAsync<T>(Stream writeStream, OpenApiSerializationContext<T> context,
            CancellationToken cancellationToken) where T : new();
    }
}