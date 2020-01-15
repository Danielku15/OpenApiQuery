namespace OpenApiQuery
{
    public class Single<T>
    {
        internal OpenApiQueryOptions<T> Options { get; }
        public T ResultItem { get; set; }

        public Single()
        {
        }

        internal Single(OpenApiQueryOptions<T> options, T resultItem)
        {
            Options = options;
            ResultItem = resultItem;
        }
    }
}
