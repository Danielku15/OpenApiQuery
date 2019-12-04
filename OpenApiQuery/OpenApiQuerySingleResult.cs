namespace OpenApiQuery
{
    public class OpenApiQuerySingleResult<T>
    {
        internal OpenApiQueryOptions<T> Options { get; set; }
        public T ResultItem { get; set; }

        public OpenApiQuerySingleResult()
        {
        }

        internal OpenApiQuerySingleResult(OpenApiQueryOptions<T> options, T resultItem)
        {
            Options = options;
            ResultItem = resultItem;
        }
    }
}
