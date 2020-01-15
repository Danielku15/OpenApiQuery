using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using OpenApiQuery.Parsing;

namespace OpenApiQuery.Serialization.SystemText
{
    public class OpenApiQueryResultConverter<T> : JsonConverter<Multiple<T>>
    {
        private const string ResultCountPropertyName = "@odata.count";
        private const string ResultValuesPropertyName = "value";
        private readonly IOpenApiTypeHandler _typeHandler;

        public OpenApiQueryResultConverter(IOpenApiTypeHandler typeHandler)
        {
            _typeHandler = typeHandler;
        }

        public override Multiple<T> Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException($"Unexpected JSON Token {reader.TokenType}.");
            }

            var actualClrType = typeof(T);

            var result = new Multiple<T>();
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    return result;
                }

                if (reader.TokenType != JsonTokenType.PropertyName)
                {
                    throw new JsonException($"Unexpected JSON Token {reader.TokenType}.");
                }

                switch (reader.GetString())
                {
                    case ResultCountPropertyName:
                        if (!reader.Read())
                        {
                            throw new JsonException("Unexpected end of stream.");
                        }

                        switch (reader.TokenType)
                        {
                            case JsonTokenType.Number:
                                result.TotalCount = reader.GetInt64();
                                break;
                            case JsonTokenType.Null:
                                result.TotalCount = null;
                                break;
                            default:
                                throw new JsonException($"Unexpected JSON Token {reader.TokenType}.");
                        }

                        break;
                    case ResultValuesPropertyName:
                        if (!reader.Read())
                        {
                            throw new JsonException("Unexpected end of stream.");
                        }

                        var document = JsonDocument.ParseValue(ref reader);
                        result.Items = (T[])JsonHelper.ReadValue(document.RootElement,
                            null,
                            actualClrType.MakeArrayType(),
                            _typeHandler
                        );

                        break;
                }
            }

            throw new JsonException("Unexpected end of stream.");
        }

        public override void Write(
            Utf8JsonWriter writer,
            Multiple<T> value,
            JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            if (value.TotalCount != null)
            {
                writer.WriteNumber(ResultCountPropertyName, value.TotalCount.Value);
            }

            writer.WritePropertyName(ResultValuesPropertyName);

            var itemType = _typeHandler.ResolveType(typeof(T));
            var selectClause = value.Options?.SelectExpand.RootSelectClause;
            JsonHelper.WriteArray(writer, value.Items, itemType, selectClause, _typeHandler, options);

            writer.WriteEndObject();
        }
    }
}
