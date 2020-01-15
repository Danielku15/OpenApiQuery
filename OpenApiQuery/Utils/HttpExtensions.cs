using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace OpenApiQuery.Utils
{
    internal static class HttpExtensions
    {
        public static bool TryGetValues(
            this IQueryCollection queryCollection,
            IEnumerable<string> keys,
            out IEnumerable<string> values)
        {
            var any = false;

            values = Enumerable.Empty<string>();

            foreach (var key in keys)
            {
                if (queryCollection.TryGetValue(key, out var v) && v.Count > 0)
                {
                    values = values.Concat(v);
                    any = true;
                }
            }

            return any;
        }
    }
}
