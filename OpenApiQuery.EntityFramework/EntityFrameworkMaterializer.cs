using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace OpenApiQuery.EntityFramework
{
    public struct EntityFrameworkMaterializer : IMaterializer
    {
        public async Task<IReadOnlyCollection<T>> MaterializeAsync<T>(IQueryable<T> queryable, CancellationToken cancellationToken) =>
            await queryable.ToArrayAsync(cancellationToken);

        public Task<T> SingleOrDefaultAsync<T>(IQueryable<T> queryable, CancellationToken cancellationToken) =>
            queryable.SingleOrDefaultAsync(cancellationToken);

        public Task<long> LongCountAsync<T>(IQueryable<T> queryable, CancellationToken cancellationToken) =>
            queryable.LongCountAsync(cancellationToken);
    }
}
