using System.Linq;

namespace OpenApiQuery
{
    public class OpenApiQueryResult
    {
        public IQueryable Queryable { get; }
        public OpenApiQueryOptions QueryOptions { get; }

        public OpenApiQueryResult(IQueryable queryable, OpenApiQueryOptions queryOptions)
        {
            Queryable = queryable;
            QueryOptions = queryOptions;
        }
    }
}