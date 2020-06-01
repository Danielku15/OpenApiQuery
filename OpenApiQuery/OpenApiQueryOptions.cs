using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenApiQuery.Utils;

namespace OpenApiQuery
{
    public abstract class OpenApiQueryOptions
    {
        public SelectExpandQueryOption SelectExpand { get; }

        public FilterQueryOption Filter { get; }

        public OrderByQueryOption OrderBy { get; }

        public SkipQueryOption Skip { get; }

        public TopQueryOption Top { get; }

        public CountQueryOption Count { get; }

        public Type ElementType { get; }

        internal HttpContext HttpContext { get; set; }
        internal ModelStateDictionary ModelState { get; set; }

        protected OpenApiQueryOptions(Type elementType)
        {
            ElementType = elementType;
            SelectExpand = new SelectExpandQueryOption(ElementType);
            Filter = new FilterQueryOption(ElementType);
            OrderBy = new OrderByQueryOption(ElementType);
            Skip = new SkipQueryOption();
            Top = new TopQueryOption();
            Count = new CountQueryOption();
        }

        public void Initialize(HttpContext httpContext, ModelStateDictionary modelState)
        {
            HttpContext = httpContext;
            ModelState = modelState;
            var logger = httpContext.RequestServices.GetRequiredService<ILogger<OpenApiQueryOptions>>();
            SelectExpand.Initialize(httpContext, logger, ModelState);
            Filter.Initialize(httpContext, logger, ModelState);
            OrderBy.Initialize(httpContext, logger, ModelState);
            Skip.Initialize(httpContext, ModelState);
            Top.Initialize(httpContext, ModelState);
            Count.Initialize(httpContext, ModelState);
        }
    }

    [OpenApiQueryParameterBinding]
    public class OpenApiQueryOptions<T, TMaterializer> : OpenApiQueryOptions
        where TMaterializer : struct, IMaterializer
    {
        public OpenApiQueryOptions()
            : base(typeof(T))
        {
        }

        public async Task<OpenApiQueryResult<T>> ApplyToAsync<TSource>(
            IQueryable<TSource> documentQueryable,
            Expression<Func<TSource, T>> map,
            CancellationToken cancellationToken)
        {
            //// The order of applying the items to the queryable is important

            //// 1. include all related items for further query option
            var selectClause = SelectExpand.GetSelectClause<T>();
            var expression = ExpressionHelper.Compose(map, selectClause);
            var queryable = documentQueryable.Select(expression);

            //// 2. sort to have the correct order for filtering and limiting
            queryable = OrderBy.ApplyTo(queryable);
            // 3. filter the items according to the user input
            queryable = Filter.ApplyTo(queryable);

            var count = Count.Value ?? false
                ? await default(TMaterializer).LongCountAsync(queryable, cancellationToken)
                : default(long?);

            // 4. apply paging on the sorted and filtered result.
            queryable = Skip.ApplyTo(queryable);
            queryable = Top.ApplyTo(queryable);

            var result = await default(TMaterializer).MaterializeAsync(queryable, cancellationToken);

            return new OpenApiQueryResult<T>(count, result);
        }

        public async Task<OpenApiQueryResult<T>> ApplyToAsync(
            IQueryable<T> queryable,
            CancellationToken cancellationToken)
        {
            // The order of applying the items to the queryable is important

            // 1. include all related items for further query options
            queryable = SelectExpand.ApplyTo(queryable);
            // 2. sort to have the correct order for filtering and limiting
            queryable = OrderBy.ApplyTo(queryable);
            // 3. filter the items according to the user input
            queryable = Filter.ApplyTo(queryable);

            var count = Count.Value ?? false
                ? await default(TMaterializer).LongCountAsync(queryable, cancellationToken)
                : default(long?);

            // 4. apply paging on the sorted and filtered result.
            queryable = Skip.ApplyTo(queryable);
            queryable = Top.ApplyTo(queryable);

            var result = await default(TMaterializer).MaterializeAsync(queryable, cancellationToken);

            return new OpenApiQueryResult<T>(count, result);
        }

        public async Task<T> ApplyToSingleAsync(
            IQueryable<T> queryable,
            CancellationToken cancellationToken)
        {
            queryable = SelectExpand.ApplyTo(queryable);

            var result = await default(TMaterializer).SingleOrDefaultAsync(queryable, cancellationToken);

            return result;
        }
    }
}
