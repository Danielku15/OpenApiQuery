using System;
using System.Collections;
using System.Text.Json;
using System.Text.Json.Serialization;
using OpenApiQuery.Parsing;
using OpenApiQuery.Utils;

namespace OpenApiQuery.Serialization.SystemText
{
    public class OpenApiQueryApplyResultConverter<T> : JsonConverter<OpenApiQueryApplyResult<T>>
    {
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
            throw new NotImplementedException();
        }

        public override void Write(
            Utf8JsonWriter writer,
            OpenApiQueryApplyResult<T> value,
            JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            if (value.TotalCount != null)
            {
                writer.WriteNumber("totalCount", value.TotalCount.Value);
            }

            writer.WritePropertyName("resultItems");

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
