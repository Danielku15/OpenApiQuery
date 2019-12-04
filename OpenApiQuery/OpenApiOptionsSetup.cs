using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using OpenApiQuery.Parsing;
using OpenApiQuery.Serialization.SystemText;

namespace OpenApiQuery
{
    internal class OpenApiOptionsSetup : IConfigureOptions<JsonOptions>
    {
        private readonly IOpenApiTypeHandler _typeHandler;

        public OpenApiOptionsSetup(IOpenApiTypeHandler typeHandler)
        {
            _typeHandler = typeHandler;
        }

        public void Configure(JsonOptions options)
        {
            options.JsonSerializerOptions.Converters.Insert(0, new OpenApiQueryResultConverterFactory(_typeHandler));
            options.JsonSerializerOptions.Converters.Insert(0, new OpenApiQueryDeltaConverterFactory(_typeHandler));
        }
    }
}
