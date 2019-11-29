using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using OpenApiQuery.Parsing;

namespace OpenApiQuery.Serialization.SystemText
{
    public class OpenApiQueryApplyResultConverter<T> : JsonConverter<OpenApiQueryApplyResult<T>>
    {
        private const string ResultCountPropertyName = "@odata.count";
        private const string ResultValuesPropertyName = "value";
        private readonly IOpenApiTypeHandler _typeHandler;

        public OpenApiQueryApplyResultConverter(IOpenApiTypeHandler typeHandler)
        {
            _typeHandler = typeHandler;
        }

        public override OpenApiQueryApplyResult<T> Read(
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

            var result = new OpenApiQueryApplyResult<T>();
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
                            throw new JsonException($"Unexpected end of stream.");
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
                            throw new JsonException($"Unexpected end of stream.");
                        }

                        switch (reader.TokenType)
                        {
                            case JsonTokenType.StartArray:
                                result.ResultItems = (T[])JsonHelper.ReadArray(ref reader,
                                    actualType,
                                    actualClrType,
                                    _typeHandler,
                                    options);
                                break;
                            case JsonTokenType.Null:
                                result.ResultItems = null;
                                break;
                            default:
                                throw new JsonException($"Unexpected JSON Token {reader.TokenType}.");
                        }

                        break;
                }
            }

            throw new JsonException($"Unexpected end of stream.");
        }

        public override void Write(
            Utf8JsonWriter writer,
            OpenApiQueryApplyResult<T> value,
            JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            if (value.TotalCount != null)
            {
                writer.WriteNumber(ResultCountPropertyName, value.TotalCount.Value);
            }

            writer.WritePropertyName(ResultValuesPropertyName);

            var itemType = _typeHandler.ResolveType(typeof(T));
            var selectClause = value.Options.SelectExpand.RootSelectClause;
            JsonHelper.WriteArray(writer, value.ResultItems, itemType, selectClause, _typeHandler, options);

            writer.WriteEndObject();
        }
    }
}
