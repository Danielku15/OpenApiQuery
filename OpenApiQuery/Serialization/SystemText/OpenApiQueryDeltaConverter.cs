using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using OpenApiQuery.Parsing;

namespace OpenApiQuery.Serialization.SystemText
{
    public class OpenApiQueryDeltaConverter<T> : JsonConverter<Delta<T>>
    {
        private readonly IOpenApiTypeHandler _typeHandler;

        public OpenApiQueryDeltaConverter(IOpenApiTypeHandler typeHandler)
        {
            _typeHandler = typeHandler;
        }

        public override Delta<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException($"Unexpected JSON Token {reader.TokenType}.");
            }

            var actualClrType = typeof(T);
            var actualType = _typeHandler.ResolveType(actualClrType);

            return (Delta<T>)JsonHelper.ReadObject(ref reader,
                actualType,
                actualClrType,
                _typeHandler,
                options,
                true
            );
        }

        public override void Write(Utf8JsonWriter writer, Delta<T> value, JsonSerializerOptions options)
        {
            if (value == null)
            {
                writer.WriteNullValue();
            }
            else
            {
                var clrType = typeof(T);
                var type = _typeHandler.ResolveType(clrType);
                if (type == null)
                {
                    throw new JsonException($"Could not resolve API type '{clrType.Name}'");
                }

                writer.WriteStartObject();

                foreach (var changedProperty in value.ChangedProperties)
                {
                    if (type.TryGetProperty(changedProperty.Key, out var property))
                    {
                        writer.WritePropertyName(property.JsonName);
                        JsonHelper.WriteValue(writer,
                            type,
                            changedProperty.Value,
                            null,
                            _typeHandler,
                            options);
                    }
                }

                writer.WriteEndObject();
            }
        }
    }
}
