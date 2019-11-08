using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace OpenApiQuery
{
    internal class OpenApiOptionsSetup : IConfigureOptions<MvcOptions>
    {
        private readonly IOptions<JsonOptions> _jsonOptions;

        public OpenApiOptionsSetup(IOptions<JsonOptions> jsonOptions)
        {
            _jsonOptions = jsonOptions;
        }
        public void Configure(MvcOptions options)
        {
            var formatter = new OpenApiQueryResultOutputFormatter(_jsonOptions.Value.JsonSerializerOptions);
            options.OutputFormatters.Insert(0, formatter);
        }
    }
}