using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.Json;
using OpenApiQuery.Parsing;
using OpenApiQuery.Utils;

namespace OpenApiQuery.Serialization.SystemText
{
    internal class JsonHelper
    {
        // TODO: theoretically we should be able to replace the JsonHelper with another JsonConverter
        // Idea: wherever we use the JsonHelper we call JsonSerializer.Serialize or Deserialize
        // but we "clone" the options and inject an additional JsonConverterFactory that handles
        // all API types and passes on the SelectClause and type information as context.
        //  OpenApiQueryApplyResultConverter.Write() {
        //     var options = CloneOptions(options);
        //     options.Converter.Insert(0, new OpenApiQueryObjectConverter(selectClause));
        //     JsonSerializer.Serialize(obj, options);
        // }
        // class OpenApiQueryObjectConverter {
        //     Write() {
        //        // write object as below
        //     }
        // }



        public static void WriteValue(
            Utf8JsonWriter writer,
            IOpenApiType itemType,
            object item,
            SelectClause subClause,
            IOpenApiTypeHandler typeHandler,
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
                case Delta d:
                    JsonSerializer.Serialize(writer, d, d.GetType(), options);
                    break;

                case IDictionary c:
                    if (ReflectionHelper.ImplementsDictionary(c.GetType(), out _, out var valueType))
                    {
                        WriteDictionary(writer,
                            c,
                            typeHandler.ResolveType(valueType), // use static types for polymorphism
                            typeHandler,
                            options);
                    }
                    else
                    {
                        WriteDictionary(writer, c, null, typeHandler, options);
                    }

                    break;
                case IEnumerable v:
                    if (ReflectionHelper.ImplementsEnumerable(v.GetType(), out var enumerableItemType))
                    {
                        WriteArray(writer,
                            v,
                            typeHandler.ResolveType(enumerableItemType),
                            subClause,
                            typeHandler,
                            options);
                    }
                    else
                    {
                        WriteArray(writer, v, null, subClause, typeHandler, options);
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
                        WriteObject(writer, itemType, item, subClause, typeHandler, options);
                    }

                    break;
            }
        }


        public static void WriteObject(
            Utf8JsonWriter writer,
            IOpenApiType itemType,
            object item,
            SelectClause selectClause,
            IOpenApiTypeHandler typeHandler,
            JsonSerializerOptions options)
        {
            if (item == null)
            {
                writer.WriteNullValue();
            }
            else
            {
                writer.WriteStartObject();

                var actualType = typeHandler.ResolveType(item.GetType());
                var needsType = itemType == null || !itemType.Equals(actualType);
                if (needsType)
                {
                    writer.WriteString("@odata.type", actualType.JsonName);
                }

                foreach (var property in actualType.Properties)
                {
                    SelectClause subClause = null;
                    if (selectClause == null ||
                        selectClause.SelectClauses?.TryGetValue(property.ClrProperty, out subClause) == true ||
                        selectClause.IsStarSelect)
                    {
                        var key = options.PropertyNamingPolicy.ConvertName(property.JsonName);
                        writer.WritePropertyName(JsonEncodedText.Encode(key));

                        WriteValue(writer,
                            typeHandler.ResolveType(property.ClrProperty.PropertyType),
                            property.GetValue(item),
                            subClause,
                            typeHandler,
                            options);
                    }
                }

                writer.WriteEndObject();
            }
        }

        public static void WriteArray(
            Utf8JsonWriter writer,
            IEnumerable resultItems,
            IOpenApiType itemType,
            SelectClause selectClause,
            IOpenApiTypeHandler typeHandler,
            JsonSerializerOptions options)
        {
            writer.WriteStartArray();

            foreach (var item in resultItems)
            {
                WriteValue(writer, itemType, item, selectClause, typeHandler, options);
            }

            writer.WriteEndArray();
        }

        public static void WriteDictionary(
            Utf8JsonWriter writer,
            IDictionary value,
            IOpenApiType valueType,
            IOpenApiTypeHandler typeHandler,
            JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            foreach (DictionaryEntry dictionaryEntry in value)
            {
                var key = options.DictionaryKeyPolicy != null
                    ? options.DictionaryKeyPolicy.ConvertName(dictionaryEntry.Key.ToString())
                    : dictionaryEntry.Key.ToString();
                writer.WritePropertyName(JsonEncodedText.Encode(key));

                WriteValue(writer, valueType, dictionaryEntry.Value, null, typeHandler, options);
            }

            writer.WriteEndObject();
        }


        public static object ReadValue(
            ref Utf8JsonReader reader,
            IOpenApiType valueType,
            Type valueClrType,
            IOpenApiTypeHandler typeHandler,
            JsonSerializerOptions options,
            bool objectsAsDelta = false)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                return null;
            }

            if (valueClrType == typeof(string))
            {
                return reader.GetString();
            }

            if (ReflectionHelper.ImplementsDictionary(valueClrType, out _, out _))
            {
                return JsonSerializer.Deserialize(ref reader, valueClrType, options);
            }
            else if (ReflectionHelper.ImplementsEnumerable(valueClrType, out var itemType))
            {
                var array = ReadArray(ref reader,
                    typeHandler.ResolveType(itemType),
                    itemType,
                    typeHandler,
                    options,
                    objectsAsDelta);
                // TODO: convert to actual target type
                return array;
            }
            else if (valueType == null)
            {
                return JsonSerializer.Deserialize(ref reader, valueClrType, options);
            }
            else
            {
                return ReadObject(ref reader, valueType, valueClrType, typeHandler, options, objectsAsDelta);
            }
        }

        public static object ReadArray(
            ref Utf8JsonReader reader,
            IOpenApiType itemType,
            Type itemClrType,
            IOpenApiTypeHandler typeHandler,
            JsonSerializerOptions options,
            bool objectsAsDelta = false)
        {
            if (reader.TokenType == JsonTokenType.StartArray && !reader.Read())
            {
                throw new JsonException($"Unexpected end of stream.");
            }

            switch (reader.TokenType)
            {
                case JsonTokenType.Null:
                    return null;
                case JsonTokenType.StartObject:
                    return ReadObjectArray(ref reader, itemType, itemClrType, typeHandler, options, objectsAsDelta);

                case JsonTokenType.StartArray:
                    if (itemClrType.IsArray)
                    {
                        var elementType = itemClrType.GetElementType();
                        return ReadArrayArray(ref reader,
                            typeHandler.ResolveType(elementType),
                            elementType,
                            typeHandler,
                            options,
                            objectsAsDelta);
                    }
                    else
                    {
                        throw new JsonException($"Unexpected JSON Token {reader.TokenType}.");
                    }
                case JsonTokenType.String:
                case JsonTokenType.Number:
                case JsonTokenType.True:
                case JsonTokenType.False:
                    return ReadNativeArray(ref reader, itemClrType, options);


                    // TODO: read array even though we already started it? we might need to do reading on our own
                    // simple arrays
                    return JsonSerializer.Deserialize(ref reader, itemClrType.MakeArrayType(), options);

                case JsonTokenType.EndArray:
                    return Array.CreateInstance(itemClrType, 0);

                default:
                    throw new JsonException($"Unexpected JSON Token {reader.TokenType}.");
            }
        }

        private static object ReadNativeArray(
            ref Utf8JsonReader reader,
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

                resultItems.Add(JsonSerializer.Deserialize(ref reader, itemClrType, options));
            } while (reader.Read());

            throw new JsonException($"Unexpected end of stream.");
        }

        private static object ReadObjectArray(
            ref Utf8JsonReader reader,
            IOpenApiType itemType,
            Type itemClrType,
            IOpenApiTypeHandler typeHandler,
            JsonSerializerOptions options,
            bool objectsAsDelta = false)
        {
            var elementType = objectsAsDelta
                ? typeof(Delta<>).MakeGenericType(itemClrType)
                : itemClrType;

            var resultItems = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(elementType));
            do
            {
                if (reader.TokenType == JsonTokenType.EndArray)
                {
                    var array = Array.CreateInstance(itemClrType, resultItems.Count);
                    resultItems.CopyTo(array, 0);
                    return array;
                }

                resultItems.Add(ReadValue(ref reader, itemType, itemClrType, typeHandler, options, objectsAsDelta));
            } while (reader.Read());

            throw new JsonException($"Unexpected end of stream.");
        }

        public static object ReadObject(
            ref Utf8JsonReader reader,
            IOpenApiType itemType,
            Type itemClrType,
            IOpenApiTypeHandler typeHandler,
            JsonSerializerOptions options,
            bool objectAsDelta = false)
        {
            if (itemType == null)
            {
                throw new JsonException($"Cannot deserialize unknown type.");
            }

            // TODO: better object creation, and polymorphism handling!
            var instance = objectAsDelta
                ? Activator.CreateInstance(typeof(Delta<>).MakeGenericType(itemClrType))
                : Activator.CreateInstance(itemClrType);

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

                if (propertyName == "@odata.type")
                {
                    // TOOD: support polymorphism
                    reader.Skip();
                }
                else if (itemType.TryGetProperty(propertyName, out var property))
                {
                    if (!reader.Read())
                    {
                        throw new JsonException("Unexpected end of stream.");
                    }

                    var propertyType = typeHandler.ResolveType(property.ClrProperty.PropertyType);
                    var value = ReadValue(ref reader,
                        propertyType,
                        property.ClrProperty.PropertyType,
                        typeHandler,
                        options,
                        objectAsDelta);
                    // TODO: check for components in the model binding area which help us here
                    // many conversions (string -> enum,date,timespan) etc. do not work like this.
                    property.SetValue(instance, value);
                }
                else
                {
                    // TODO: how do we want to treat unknown props? fail or ignore?
                    // throw new JsonException($"Unexpected property {propertyName}.");
                    reader.Skip();
                }
            }

            throw new JsonException($"Unexpected end of stream.");
        }


        private static object ReadArrayArray(
            ref Utf8JsonReader reader,
            IOpenApiType itemType,
            Type itemClrType,
            IOpenApiTypeHandler typeHandler,
            JsonSerializerOptions options,
            bool objectsAsDelta = false)
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

                resultItems.Add(ReadArray(ref reader, itemType, itemClrType, typeHandler, options, objectsAsDelta));
            } while (reader.Read());

            throw new JsonException($"Unexpected end of stream.");
        }
    }
}
