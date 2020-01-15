using System.Collections.Generic;

namespace OpenApiQuery
{
    public static class QueryOptionKeys
    {
        public static readonly ISet<string> SelectKeys = new HashSet<string>
        {
            "select",
            "$select"
        };

        public static readonly ISet<string> CountKeys = new HashSet<string>
        {
            "count",
            "$count"
        };

        public static readonly ISet<string> FilterKeys = new HashSet<string>
        {
            "filter",
            "$filter"
        };

        public static readonly ISet<string> ExpandKeys = new HashSet<string>
        {
            "expand",
            "$expand"
        };

        public static readonly ISet<string> OrderbyKeys = new HashSet<string>
        {
            "orderby",
            "$orderby"
        };

        public static readonly ISet<string> TopKeys = new HashSet<string>
        {
            "top",
            "$top"
        };

        public static readonly ISet<string> SkipKeys = new HashSet<string>
        {
            "skip",
            "$skip"
        };
    }
}
