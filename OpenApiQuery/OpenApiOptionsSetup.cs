using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using OpenApiQuery.Serialization;

namespace OpenApiQuery
{
    internal class OpenApiOptionsSetup : IConfigureOptions<MvcOptions>
    {
        public void Configure(MvcOptions options)
        {
            var formatter = new OpenApiQueryResultOutputFormatter();
            options.OutputFormatters.Insert(0, formatter);
        }
    }
}