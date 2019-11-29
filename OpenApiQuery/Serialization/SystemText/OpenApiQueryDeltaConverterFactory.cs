using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using OpenApiQuery.Parsing;
using OpenApiQuery.Utils;

namespace OpenApiQuery.Serialization.SystemText
{
    public class OpenApiQueryDeltaConverterFactory : JsonConverterFactory
    {
        private readonly IOpenApiTypeHandler _typeHandler;

        public OpenApiQueryDeltaConverterFactory(IOpenApiTypeHandler typeHandler)
        {
            _typeHandler = typeHandler;
        }

        public override bool CanConvert(Type typeToConvert)
        {
            return typeToConvert.IsGenericType &&
                   typeToConvert.GetGenericTypeDefinition() == typeof(Delta<>);
        }

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            return CreateConverterOfT.Invoke(this, typeToConvert.GetGenericArguments()[0]);
        }

        private JsonConverter CreateConverter<T>()
        {
            return new OpenApiQueryDeltaConverter<T>(_typeHandler);
        }

        private static readonly
            GenericMethodCallHelper<OpenApiQueryDeltaConverterFactory, JsonConverter>
            CreateConverterOfT =
                new GenericMethodCallHelper<OpenApiQueryDeltaConverterFactory, JsonConverter>(
                    x => x.CreateConverter<object>()
                );
    }
}
