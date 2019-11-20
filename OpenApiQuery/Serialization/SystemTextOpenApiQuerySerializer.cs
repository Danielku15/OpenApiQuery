using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using OpenApiQuery.Utils;

namespace OpenApiQuery.Serialization
{
    public class SystemTextOpenApiQuerySerializer : IOpenApiQuerySerializer
    {
        public JsonSerializerOptions SerializerOptions { get; }

        public SystemTextOpenApiQuerySerializer(IOptions<JsonOptions> jsonOptions)
        {
            SerializerOptions = jsonOptions.Value.JsonSerializerOptions;
        }

        public async Task SerializeResultAsync<T>(Stream writeStream, OpenApiQueryResult queryResult,
            OpenApiQueryApplyResult<T> appliedQueryResult,
            CancellationToken cancellationToken) where T : new()
        {
            await using (var writer = new Utf8JsonWriter(writeStream, new JsonWriterOptions
            {
                Encoder = SerializerOptions.Encoder,
                Indented = SerializerOptions.WriteIndented,
                SkipValidation = true
            }))
            {
                writer.WriteStartObject();

                if (appliedQueryResult.TotalCount != null)
                {
                    writer.WriteNumber("totalCount", appliedQueryResult.TotalCount.Value);
                }

                writer.WritePropertyName("resultItems");

                await WriteArrayAsync(writer, appliedQueryResult.ResultItems,
                    typeof(T),
                    queryResult.QueryOptions.SelectExpand.RootSelectClause, cancellationToken);

                writer.WriteEndObject();
            }
        }

        private async Task WriteArrayAsync(Utf8JsonWriter writer, IEnumerable resultItems,
            Type itemType,
            SelectClause selectClause,
            CancellationToken cancellationToken)
        {
            writer.WriteStartArray();

            foreach (var item in resultItems)
            {
                await WriteObjectAsync(writer, itemType, item, selectClause, cancellationToken);
            }

            writer.WriteEndArray();
        }

        private async Task WriteObjectAsync(Utf8JsonWriter writer, Type itemType, object item,
            SelectClause selectClause, CancellationToken cancellationToken)
        {
            if (item == null)
            {
                writer.WriteNullValue();
            }
            else
            {
                writer.WriteStartObject();

                var actualType = item.GetType();
                var needsType = itemType != actualType;
                if (needsType)
                {
                    // TODO: ask injectable component for serialization type name. 
                    writer.WriteString("@odata.type", actualType.FullName);
                }

                // TODO: caching and allow manual filtering on top (e.g. attributes) 
                foreach (var property in actualType.GetProperties(BindingFlags.Instance | BindingFlags.Public))
                {
                    SelectClause subClause = null;
                    if (selectClause == null ||
                        selectClause.SelectClauses?.TryGetValue(property, out subClause) == true ||
                        selectClause.IsStarSelect)
                    {
                        var jsonPropertyName = SerializerOptions.PropertyNamingPolicy.ConvertName(property.Name);
                        writer.WritePropertyName(jsonPropertyName);
                        await WriteValueAsync(writer, property.PropertyType, property.GetValue(item), subClause, cancellationToken);
                    }
                }

                writer.WriteEndObject();
            }
        }

        private async Task WriteValueAsync(Utf8JsonWriter writer, Type itemType, object item, SelectClause subClause,
            CancellationToken cancellationToken)
        {
            // TODO: max levels detection
            switch (item)
            {
                case null:
                    writer.WriteNullValue();
                    break;
                // integral types
                case sbyte v:
                    writer.WriteNumberValue(v);
                    break;
                case byte v:
                    writer.WriteNumberValue(v);
                    break;
                case short v:
                    writer.WriteNumberValue(v);
                    break;
                case ushort v:
                    writer.WriteNumberValue(v);
                    break;
                case int v:
                    writer.WriteNumberValue(v);
                    break;
                case uint v:
                    writer.WriteNumberValue(v);
                    break;
                case long v:
                    writer.WriteNumberValue(v);
                    break;
                case ulong v:
                    writer.WriteNumberValue(v);
                    break;
                // double types
                case float v:
                    writer.WriteNumberValue(v);
                    break;
                case double v:
                    writer.WriteNumberValue(v);
                    break;
                case decimal v:
                    writer.WriteNumberValue(v);
                    break;
                // other system types
                case bool v:
                    writer.WriteBooleanValue(v);
                    break;
                case char v:
                    writer.WriteStringValue("" + v);
                    break;
                case string v:
                    writer.WriteStringValue(v);
                    break;
                // some special CLR types
                case DateTime v:
                    writer.WriteStringValue(v);
                    break;
                case DateTimeOffset v:
                    writer.WriteStringValue(v);
                    break;
                case TimeSpan v:
                    writer.WriteStringValue(v.ToString("c"));
                    break;
                case IDictionary v:
                    // TODO dictionary
                    break;
                case IEnumerable v:
                    var vt = v.GetType();
                    if (ReflectionHelper.ImplementsEnumerable(vt, out var enumerableItemType))
                    {
                        await WriteArrayAsync(writer, v, enumerableItemType, subClause, cancellationToken);
                    }
                    else
                    {
                        await WriteArrayAsync(writer, v, typeof(object), subClause, cancellationToken);
                    }
                    break;
                // sub-objects
                default:
                    // TODO: cyclic reference detection
                    await WriteObjectAsync(writer, itemType, item, subClause, cancellationToken);
                    break;
            }
        }
    }
}