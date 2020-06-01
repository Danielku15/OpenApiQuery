using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OpenApiQuery
{
    public interface IMaterializer
    {
        Task<IReadOnlyCollection<T>> MaterializeAsync<T>(IQueryable<T> queryable, CancellationToken cancellationToken);
        Task<T> SingleOrDefaultAsync<T>(IQueryable<T> queryable, CancellationToken cancellationToken);
        Task<long> LongCountAsync<T>(IQueryable<T> queryable, CancellationToken cancellationToken);
    }
}
