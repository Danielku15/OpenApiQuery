using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using OpenApiQuery.Parsing;
using OpenApiQuery.Utils;

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
                                result.ResultItems = (T[])ReadArray(ref reader, actualType, actualClrType, options);
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

        private object ReadArray(
            ref Utf8JsonReader reader,
            IOpenApiType itemType,
            Type itemClrType,
            JsonSerializerOptions options)
        {
            if (!reader.Read())
            {
                throw new JsonException($"Unexpected end of stream.");
            }

            switch (reader.TokenType)
            {
                case JsonTokenType.Null:
                    return null;
                case JsonTokenType.StartObject:
                    return ReadObjectArray(ref reader, itemType, itemClrType, options);

                case JsonTokenType.StartArray:
                    if (itemClrType.IsArray)
                    {
                        var elementType = itemClrType.GetElementType();
                        return ReadArrayArray(ref reader,
                            _typeHandler.ResolveType(elementType),
                            elementType,
                            options);
                    }
                    else
                    {
                        throw new JsonException($"Unexpected JSON Token {reader.TokenType}.");
                    }
                case JsonTokenType.String:
                case JsonTokenType.Number:
                case JsonTokenType.True:
                case JsonTokenType.False:
                    // TODO: read array even though we already started it? we might need to do reading on our own
                    // simple arrays
                    return JsonSerializer.Deserialize(ref reader, itemClrType.MakeArrayType(), options);

                case JsonTokenType.EndArray:
                    return Array.CreateInstance(itemClrType, 0);

                default:
                    throw new JsonException($"Unexpected JSON Token {reader.TokenType}.");
            }
        }

        private object ReadObjectArray(
            ref Utf8JsonReader reader,
            IOpenApiType itemType,
            Type itemClrType,
            JsonSerializerOptions options)
        {
            var resultItems = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(itemClrType));
            do
            {
                if (reader.TokenType == JsonTokenType.EndArray)
                {
                    var array = Array.CreateInstance(itemClrType, resultItems.Count);
                    resultItems.CopyTo(array, 0);
                    return array;
                }

                resultItems.Add(ReadObject(ref reader, itemType, itemClrType, options));
            } while (reader.Read());

            throw new JsonException($"Unexpected end of stream.");
        }

        private object ReadArrayArray(
            ref Utf8JsonReader reader,
            IOpenApiType itemType,
            Type itemClrType,
            JsonSerializerOptions options)
        {
            var resultItems = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(itemClrType));
            do
            {
                if (reader.TokenType == JsonTokenType.EndArray)
                {
                    var array = Array.CreateInstance(itemClrType, resultItems.Count);
                    resultItems.CopyTo(array, 0);
                    return array;
                }

                resultItems.Add(ReadArray(ref reader, itemType, itemClrType, options));
            } while (reader.Read());

            throw new JsonException($"Unexpected end of stream.");
        }


        private object ReadObject(
            ref Utf8JsonReader reader,
            IOpenApiType itemType,
            Type itemClrType,
            JsonSerializerOptions options)
        {
            if (itemType == null)
            {
                throw new JsonException($"Cannot deserialize unknown type.");
            }

            // TODO: better object creation, and polymorphism handling!
            var instance = Activator.CreateInstance(itemClrType);

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    return instance;
                }

                if (reader.TokenType != JsonTokenType.PropertyName)
                {
                    throw new JsonException($"Unexpected JSON Token {reader.TokenType}.");
                }

                var propertyName = reader.GetString();

                if (itemType.TryGetProperty(propertyName, out var property))
                {
                    var propertyType = _typeHandler.ResolveType(property.ValueType);
                    var value = ReadValue(ref reader, propertyType, property.ValueType, options);
                    property.SetProperty(instance, value);
                }
                else
                {
                    throw new JsonException($"Unexpected property {propertyName}.");
                }
            }

            throw new JsonException($"Unexpected end of stream.");
        }

        private object ReadValue(
            ref Utf8JsonReader reader,
            IOpenApiType valueType,
            Type valueClrType,
            JsonSerializerOptions options)
        {
            // TODO: dictionary and enumerable handling
            if (!reader.Read())
            {
                throw new JsonException($"Unexpected end of stream.");
            }

            if (reader.TokenType == JsonTokenType.Null)
            {
                return null;
            }

            if (valueClrType == typeof(string))
            {
                return reader.GetString();
            }
            if (ReflectionHelper.ImplementsEnumerable(valueClrType, out var itemType))
            {

                var array = ReadArray(ref reader, _typeHandler.ResolveType(itemType), itemType, options);
                // TODO: convert to actual target type
                return array;
            }
            else if (ReflectionHelper.ImplementsDictionary(valueClrType, out var dictKeyType, out var dictValueType))
            {
                // TODO: dictionarySupport
                return null;
            }
            else if (valueType == null)
            {
                return JsonSerializer.Deserialize(ref reader, valueClrType, options);
            }
            else
            {
                return ReadObject(ref reader, valueType, valueClrType, options);
            }
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
            WriteArray(writer, value.ResultItems, itemType, selectClause, options);

            writer.WriteEndObject();
        }


        private void WriteArray(
            Utf8JsonWriter writer,
            IEnumerable resultItems,
            IOpenApiType itemType,
            SelectClause selectClause,
            JsonSerializerOptions options)
        {
            writer.WriteStartArray();

            foreach (var item in resultItems)
            {
                WriteValue(writer, itemType, item, selectClause, options);
            }

            writer.WriteEndArray();
        }

        private void WriteObject(
            Utf8JsonWriter writer,
            IOpenApiType itemType,
            object item,
            SelectClause selectClause,
            JsonSerializerOptions options)
        {
            if (item == null)
            {
                writer.WriteNullValue();
            }
            else
            {
                writer.WriteStartObject();

                var actualType = _typeHandler.ResolveType(item.GetType());
                var needsType = itemType == null || !itemType.Equals(actualType);
                if (needsType)
                {
                    writer.WriteString("@odata.type", actualType.JsonName);
                }

                foreach (var property in actualType.Properties.Values)
                {
                    SelectClause subClause = null;
                    if (selectClause == null ||
                        selectClause.SelectClauses?.TryGetValue(property.ClrProperty, out subClause) == true ||
                        selectClause.IsStarSelect)
                    {
                        var jsonPropertyName = property.JsonName;
                        writer.WritePropertyName(jsonPropertyName);
                        WriteValue(writer,
                            _typeHandler.ResolveType(property.ValueType),
                            property.GetValue(item),
                            subClause,
                            options);
                    }
                }

                writer.WriteEndObject();
            }
        }


        private void WriteDictionary(
            Utf8JsonWriter writer,
            IDictionary value,
            IOpenApiType valueType,
            JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            foreach (DictionaryEntry dictionaryEntry in value)
            {
                var key = options.DictionaryKeyPolicy.ConvertName(dictionaryEntry.Key.ToString());
                writer.WritePropertyName(JsonEncodedText.Encode(key));

                WriteValue(writer, valueType, dictionaryEntry.Value, null, options);
            }


            writer.WriteEndObject();
        }

        private void WriteValue(
            Utf8JsonWriter writer,
            IOpenApiType itemType,
            object item,
            SelectClause subClause,
            JsonSerializerOptions options)
        {
            switch (item)
            {
                // we must handle dictionaries, enumerables etc on our own for proper nested object handling
                // maybe we find a better way to utilize the JsonSerializer.Serialize directly without loosing the proper type handling
                case string s:
                    // string is an IEnumerable and needs to be handled special to avoid serialization as array
                    writer.WriteStringValue(s);
                    break;
                case IDictionary c:
                    if (ReflectionHelper.ImplementsDictionary(c.GetType(), out _, out var valueType))
                    {
                        WriteDictionary(writer,
                            c,
                            _typeHandler.ResolveType(valueType), // use static types for polymorphism
                            options);
                    }
                    else
                    {
                        WriteDictionary(writer, c, null, options);
                    }

                    break;
                case IEnumerable v:
                    if (ReflectionHelper.ImplementsEnumerable(v.GetType(), out var enumerableItemType))
                    {
                        WriteArray(writer, v, _typeHandler.ResolveType(enumerableItemType), subClause, options);
                    }
                    else
                    {
                        WriteArray(writer, v, null, subClause, options);
                    }

                    break;
                default:
                    if (itemType == null)
                    {
                        // not handled by OpenApiQuery
                        JsonSerializer.Serialize(writer, item, options);
                    }
                    else
                    {
                        WriteObject(writer, itemType, item, subClause, options);
                    }

                    break;
            }
        }
    }
}
