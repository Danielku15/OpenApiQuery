using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenApiQuery.Parsing;
using OpenApiQuery.Serialization.SystemText;

namespace OpenApiQuery.Test.Serialization.SystemText
{
    public class SerializationTestBase
    {
        protected JsonDocument Serialize<T>(T obj)
        {
            var typeHandler = new DefaultOpenApiTypeHandler();
            var jsonSerializerOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Converters =
                {
                    new OpenApiQueryDeltaConverterFactory(typeHandler),
                    new OpenApiQueryResultConverterFactory(typeHandler),
                    new OpenApiQuerySingleResultConverterFactory(typeHandler)
                }
            };

            var json = JsonSerializer.Serialize(obj, jsonSerializerOptions);
            return JsonDocument.Parse(json, new JsonDocumentOptions());
        }

        protected T Deserialize<T>(string json, IOpenApiTypeHandler typeHandler = null)
        {
            if (typeHandler == null)
            {
                typeHandler = new DefaultOpenApiTypeHandler();
            }
            var jsonSerializerOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Converters =
                {
                    new OpenApiQueryDeltaConverterFactory(typeHandler),
                    new OpenApiQueryResultConverterFactory(typeHandler),
                    new OpenApiQuerySingleResultConverterFactory(typeHandler)
                }
            };

            return JsonSerializer.Deserialize<T>(json, jsonSerializerOptions);
        }

        protected void VerifySerialize<T>(Dictionary<string, object> expected, T actual)
        {
            var expectedJson = Serialize(expected);
            var actualJson = Serialize(actual);
            AreEqual(expectedJson, actualJson);
        }

        private void AreEqual(JsonDocument expected, JsonDocument actual)
        {
            AreEqual(expected.RootElement, actual.RootElement, "/");
        }

        private void AreEqual(JsonElement expected, JsonElement actual, string relativePath)
        {
            Assert.AreEqual(expected.ValueKind,
                actual.ValueKind,
                JsonExpectationError("ValueKinds do not match", relativePath));

            switch (expected.ValueKind)
            {
                case JsonValueKind.Undefined:
                case JsonValueKind.Null:
                case JsonValueKind.True:
                case JsonValueKind.False:
                case JsonValueKind.Number:
                    // no handling needed
                    break;
                case JsonValueKind.Object:
                    var eObj = expected.EnumerateObject().ToDictionary(p => p.Name, p => p.Value);
                    var aObj = actual.EnumerateObject().ToDictionary(p => p.Name, p => p.Value);

                    Assert.AreEqual(string.Join(",", eObj.Keys.OrderBy(k=>k)),
                        string.Join(",", aObj.Keys.OrderBy(k=>k)),
                        JsonExpectationError("Object does not contain same keys", relativePath));


                    foreach (var key in eObj.Keys)
                    {
                        var eVal = eObj[key];
                        var aVal = aObj[key];

                        AreEqual(eVal, aVal, $"{relativePath}/{key}");
                    }

                    break;
                case JsonValueKind.Array:
                    var eArrayLength = expected.GetArrayLength();
                    var aArrayLength = actual.GetArrayLength();
                    Assert.AreEqual(eArrayLength,
                        aArrayLength,
                        JsonExpectationError("Array lengths values do not match", relativePath));

                    for (var i = 0; i < aArrayLength; i++)
                    {
                        var eValue = expected[i];
                        var aValue = actual[i];

                        AreEqual(eValue, aValue, $"{relativePath}[{i}]");
                    }

                    break;
                case JsonValueKind.String:
                    Assert.AreEqual(expected.GetString(),
                        actual.GetString(),
                        JsonExpectationError("String values do not match", relativePath));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private string JsonExpectationError(string message, string relativePath)
        {
            return $"'{message}' at path {relativePath}";
        }
    }
}
