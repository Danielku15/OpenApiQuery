using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace OpenApiQuery.Mongo
{
    public struct MongoMaterializer : IMaterializer
    {
        public async Task<IReadOnlyCollection<T>>  MaterializeAsync<T>(IQueryable<T> queryable, CancellationToken cancellationToken) =>
            await ((IMongoQueryable<T>) queryable).ToListAsync(cancellationToken);

        public Task<T> SingleOrDefaultAsync<T>(IQueryable<T> queryable, CancellationToken cancellationToken)
            => ((IMongoQueryable<T>) queryable).SingleOrDefaultAsync(cancellationToken);

        public Task<long> LongCountAsync<T>(IQueryable<T> queryable, CancellationToken cancellationToken)
            => ((IMongoQueryable<T>) queryable).LongCountAsync(cancellationToken);
    }
}
