using System;
using System.Collections;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using OpenApiQuery.Parsing;
using OpenApiQuery.Utils;

namespace OpenApiQuery.Serialization
{
    public class SystemTextOpenApiQuerySerializer : IOpenApiQuerySerializer
    {
        private IOpenApiTypeHandler _typeHandler;
        public JsonSerializerOptions SerializerOptions { get; }

        public SystemTextOpenApiQuerySerializer(IOptions<JsonOptions> jsonOptions, IOpenApiTypeHandler typeHandler)
        {
            _typeHandler = typeHandler;
            SerializerOptions = jsonOptions.Value.JsonSerializerOptions;
        }

        public async Task SerializeResultAsync<T>(Stream writeStream, OpenApiSerializationContext<T> context,
            CancellationToken cancellationToken) where T : new()
        {
            await using var writer = new Utf8JsonWriter(writeStream, new JsonWriterOptions
            {
                Encoder = SerializerOptions.Encoder,
                Indented = SerializerOptions.WriteIndented,
                SkipValidation = true
            });
            writer.WriteStartObject();

            if (context.ApplyResult.TotalCount != null)
            {
                writer.WriteNumber("totalCount", context.ApplyResult.TotalCount.Value);
            }

            writer.WritePropertyName("resultItems");

            var itemType = _typeHandler.ResolveType(typeof(T));

            await WriteArrayAsync(context, writer,
                context.ApplyResult.ResultItems,
                itemType,
                context.QueryResult.QueryOptions.SelectExpand.RootSelectClause,
                cancellationToken);

            writer.WriteEndObject();
        }

        private async Task WriteArrayAsync(
            OpenApiSerializationContext context,
            Utf8JsonWriter writer,
            IEnumerable resultItems,
            IOpenApiType itemType,
            SelectClause selectClause,
            CancellationToken cancellationToken)
        {
            writer.WriteStartArray();

            foreach (var item in resultItems)
            {
                await WriteObjectAsync(context, writer, itemType, item, selectClause, cancellationToken);
            }

            writer.WriteEndArray();
        }

        private async Task WriteObjectAsync(
            OpenApiSerializationContext context,
            Utf8JsonWriter writer,
            IOpenApiType itemType,
            object item,
            SelectClause selectClause,
            CancellationToken cancellationToken)
        {
            if (item == null)
            {
                writer.WriteNullValue();
            }
            else
            {
                writer.WriteStartObject();

                var actualType = _typeHandler.ResolveType(item.GetType());
                var needsType = !itemType.Equals(actualType);
                if (needsType)
                {
                    writer.WriteString("@odata.type", itemType.JsonName);
                }

                foreach (var property in itemType.Properties.Values)
                {
                    SelectClause subClause = null;
                    if (selectClause == null ||
                        selectClause.SelectClauses?.TryGetValue(property.ClrProperty, out subClause) == true ||
                        selectClause.IsStarSelect)
                    {
                        var jsonPropertyName = property.JsonName;
                        writer.WritePropertyName(jsonPropertyName);
                        await WriteValueAsync(context,
                            writer,
                            _typeHandler.ResolveType(property.ValueType),
                            property.GetValue(item), subClause,
                            cancellationToken);
                    }
                }

                writer.WriteEndObject();
            }
        }

        private async Task WriteValueAsync(
            OpenApiSerializationContext context,
            Utf8JsonWriter writer,
            IOpenApiType itemType,
            object item,
            SelectClause subClause,
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
                case IDictionary _:
                    // TODO dictionary
                    break;
                case IEnumerable v:
                    var vt = v.GetType();
                    if (ReflectionHelper.ImplementsEnumerable(vt, out var enumerableItemType))
                    {
                        await WriteArrayAsync(context, writer, v, _typeHandler.ResolveType(enumerableItemType), subClause, cancellationToken);
                    }
                    else
                    {
                        await WriteArrayAsync(context, writer, v, _typeHandler.ResolveType(typeof(object)), subClause, cancellationToken);
                    }

                    break;
                // sub-objects
                default:
                    await WriteObjectAsync(context, writer, itemType, item, subClause, cancellationToken);
                    break;
            }
        }
    }
}
