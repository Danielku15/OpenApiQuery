namespace OpenApiQuery
{
    public class Multiple<T>
    {
        internal OpenApiQueryOptions<T> Options { get; }

        public long? TotalCount { get; set; }
        public T[] Items { get; set; }

        public Multiple()
        {
        }

        internal Multiple(OpenApiQueryOptions<T> options, T[] items, long? totalCount)
        {
            Options = options;
            Items = items;
            TotalCount = totalCount;
        }
    }
}
