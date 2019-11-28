using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OpenApiQuery.Utils
{
    internal static class AsyncQueryableExtensions
    {
        // TODO: how to do this without referencing Entity Framework?
        public static Task<T[]> ToArrayAsync<T>(this IQueryable<T> queryable, CancellationToken cancellationToken)
        {
            return Task.FromResult(queryable.ToArray());
        }

        public static Task<long> LongCountAsync<T>(this IQueryable<T> queryable, CancellationToken cancellationToken)
        {
            return Task.FromResult(queryable.LongCount());
        }
    }
}
