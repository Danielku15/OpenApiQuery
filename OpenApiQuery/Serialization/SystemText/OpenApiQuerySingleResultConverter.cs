using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using OpenApiQuery.Parsing;

namespace OpenApiQuery.Serialization.SystemText
{
    public class OpenApiQuerySingleResultConverter<T> : JsonConverter<OpenApiQuerySingleResult<T>>
    {
        private readonly IOpenApiTypeHandler _typeHandler;

        public OpenApiQuerySingleResultConverter(IOpenApiTypeHandler typeHandler)
        {
            _typeHandler = typeHandler;
        }

        public override OpenApiQuerySingleResult<T> Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException($"Unexpected JSON Token {reader.TokenType}.");
            }

            var actualClrType = typeof(T);
            var actualType = _typeHandler.ResolveType(actualClrType);

            var document = JsonDocument.ParseValue(ref reader);
            var result = new OpenApiQuerySingleResult<T>
            {
                ResultItem = (T)JsonHelper.ReadValue(document.RootElement,
                    actualType,
                    actualClrType,
                    _typeHandler)
            };
            return result;
        }

        public override void Write(
            Utf8JsonWriter writer,
            OpenApiQuerySingleResult<T> value,
            JsonSerializerOptions options)
        {
            var itemType = _typeHandler.ResolveType(typeof(T));
            var selectClause = value.Options?.SelectExpand.RootSelectClause;
            JsonHelper.WriteObject(writer, itemType, value.ResultItem, selectClause, _typeHandler, options);
        }
    }
}
