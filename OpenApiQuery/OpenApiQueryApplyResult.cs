namespace OpenApiQuery
{
    public class OpenApiQueryApplyResult<T>
    {
        public OpenApiQueryOptions<T> Options { get; set; }
        public long? TotalCount { get; set; }
        public T[] ResultItems { get; set; }

        public OpenApiQueryApplyResult()
        {
        }

        public OpenApiQueryApplyResult(OpenApiQueryOptions<T> options, T[] resultItems, long? totalCount)
        {
            Options = options;
            ResultItems = resultItems;
            TotalCount = totalCount;
        }
    }
}
