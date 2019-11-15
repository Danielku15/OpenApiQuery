namespace OpenApiQuery
{
    public class OpenApiQueryApplyResult<T>
    {
        public long? TotalCount { get; set; }
        public T[] ResultItems { get; set; }

        public OpenApiQueryApplyResult()
        {
        }
        
        public OpenApiQueryApplyResult(T[] resultItems, long? totalCount)
        {
            ResultItems = resultItems;
            TotalCount = totalCount;
        }
    }
}