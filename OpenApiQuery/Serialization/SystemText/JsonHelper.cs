using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.Json;
using OpenApiQuery.Parsing;
using OpenApiQuery.Utils;

namespace OpenApiQuery.Serialization.SystemText
{
    internal class JsonHelper
    {
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

        public static object ReadObject(
            JsonElement obj,
            IOpenApiType itemType,
            Type itemClrType,
            IOpenApiTypeHandler typeHandler,
            bool objectAsDelta = false)
        {
            if (itemType == null)
            {
                throw new SerializationException("Cannot deserialize unknown type.");
            }

            var properties = obj.EnumerateObject().ToDictionary(p => p.Name, p => p.Value);

            if (properties.TryGetValue("@odata.type", out var type) && type.ValueKind == JsonValueKind.String)
            {
                var typeName = type.GetString();
                var actualType = typeHandler.ResolveType(typeName);
                if (actualType == null)
                {
                    throw new SerializationException($"No type with identifier '{typeName}' found");
                }

                if (!itemClrType.IsAssignableFrom(actualType.ClrType))
                {
                    throw new SerializationException($"Type '{typeName}' is not assignable to '{itemType.JsonName}'");
                }

                itemType = actualType;
                itemClrType = actualType.ClrType;
            }

            var instance = objectAsDelta
                ? Activator.CreateInstance(typeof(Delta<>).MakeGenericType(itemClrType))
                : Activator.CreateInstance(itemClrType);

            foreach (var member in properties)
            {
                if (itemType.TryGetProperty(member.Key, out var property))
                {
                    var propertyType = typeHandler.ResolveType(property.ClrProperty.PropertyType);

                    var value = ReadValue(member.Value,
                        propertyType,
                        property.ClrProperty.PropertyType,
                        typeHandler,
                        objectAsDelta);
                    // TODO: check for components in the model binding area which help us here
                    // many conversions (string -> enum,date,timespan) etc. do not work like this.
                    property.SetValue(instance, value);
                }
                else
                {
                    // TODO: how do we want to treat unknown props? fail or ignore?
                }
            }

            return instance;
        }

        public static object ReadValue(
            JsonElement value,
            IOpenApiType valueType,
            Type valueClrType,
            IOpenApiTypeHandler typeHandler,
            bool objectsAsDelta = false)
        {
            switch (value.ValueKind)
            {
                case JsonValueKind.Object:
                    if (valueType == null &&
                        ReflectionHelper.ImplementsDictionary(valueClrType, out var dictKeyType, out var dictValueType))
                    {
                        var dictionary = Activator.CreateInstance(valueClrType);
                        var indexer = valueClrType.GetProperties()
                            .FirstOrDefault(p => IsDictionaryIndexer(p, dictKeyType, dictValueType));
                        if (indexer == null)
                        {
                            throw new SerializationException(
                                "Could not find dictionary indexer for deserializing object");
                        }

                        var dictValueApiType = typeHandler.ResolveType(dictValueType);
                        foreach (var prop in value.EnumerateObject())
                        {
                            var dictValue = ReadValue(prop.Value,
                                dictValueApiType,
                                dictValueType,
                                typeHandler,
                                false);

                            indexer.SetValue(dictionary,
                                dictValue,
                                new object[]
                                {
                                    prop.Name
                                });
                        }

                        return dictionary;
                    }
                    else
                    {
                        return ReadObject(value, valueType, valueClrType, typeHandler, objectsAsDelta);
                    }
                case JsonValueKind.Array:
                    if (ReflectionHelper.ImplementsEnumerable(valueClrType, out var itemType))
                    {
                        var array = ReadArray(value,
                            typeHandler.ResolveType(itemType),
                            itemType,
                            typeHandler,
                            objectsAsDelta);

                        if (valueClrType.IsArray)
                        {
                            return array;
                        }

                        return Activator.CreateInstance(typeof(List<>).MakeGenericType(itemType), array);
                    }
                    else
                    {
                        var itemTypeName = valueType?.JsonName ?? valueClrType.Name;
                        throw new SerializationException($"Cannot convert an array value to {itemTypeName}");
                    }
                case JsonValueKind.Number:
                    if (valueClrType == typeof(double))
                    {
                        return value.GetDouble();
                    }
                    else if (valueClrType == typeof(float))
                    {
                        return value.GetSingle();
                    }
                    else if (valueClrType == typeof(decimal))
                    {
                        return value.GetDecimal();
                    }
                    else if (valueClrType == typeof(ulong))
                    {
                        return value.GetUInt64();
                    }
                    else if (valueClrType == typeof(long))
                    {
                        return value.GetInt64();
                    }
                    else if (valueClrType == typeof(uint))
                    {
                        return value.GetUInt32();
                    }
                    else if (valueClrType == typeof(int))
                    {
                        return value.GetInt32();
                    }
                    else if (valueClrType == typeof(ushort))
                    {
                        return value.GetUInt16();
                    }
                    else if (valueClrType == typeof(short))
                    {
                        return value.GetInt16();
                    }
                    else if (valueClrType == typeof(byte))
                    {
                        return value.GetByte();
                    }
                    else if (valueClrType == typeof(sbyte))
                    {
                        return value.GetSByte();
                    }
                    else if (valueClrType == typeof(char))
                    {
                        return (char)value.GetUInt64();
                    }
                    else
                    {
                        return Convert.ChangeType(value.GetInt32(), valueClrType);
                    }

                case JsonValueKind.Null:
                    return null;
                case JsonValueKind.String:
                    return Convert.ChangeType(value.GetString(), valueClrType);
                case JsonValueKind.True:
                    return Convert.ChangeType(true, valueClrType);
                case JsonValueKind.False:
                    return Convert.ChangeType(false, valueClrType);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static object ReadArray(JsonElement value, IOpenApiType itemApiType, Type itemType, IOpenApiTypeHandler typeHandler, bool objectsAsDelta)
        {
            var array = Array.CreateInstance(itemType, value.GetArrayLength());

            var i = 0;
            foreach (var arrayItem in value.EnumerateArray())
            {
                var arrayValue = ReadValue(arrayItem,
                    itemApiType,
                    itemType,
                    typeHandler,
                    objectsAsDelta);
                array.SetValue(arrayValue, i);
                i++;
            }


            return array;
        }

        private static bool IsDictionaryIndexer(PropertyInfo prop, Type keyType, Type valueType)
        {
            var indexerParams = prop.GetIndexParameters();
            if (indexerParams.Length != 1)
            {
                return false;
            }

            if (indexerParams[0].ParameterType != keyType)
            {
                return false;
            }

            return prop.PropertyType == valueType;
        }
    }
}
