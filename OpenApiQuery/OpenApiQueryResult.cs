namespace OpenApiQuery
{
    public class OpenApiQueryResult<T>
    {
        internal OpenApiQueryOptions<T> Options { get; set; }
        public long? TotalCount { get; set; }
        public T[] ResultItems { get; set; }

        public OpenApiQueryResult()
        {
        }

        internal OpenApiQueryResult(OpenApiQueryOptions<T> options, T[] resultItems, long? totalCount)
        {
            Options = options;
            ResultItems = resultItems;
            TotalCount = totalCount;
        }
    }
}
