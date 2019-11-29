using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using OpenApiQuery.Parsing;
using OpenApiQuery.Utils;

namespace OpenApiQuery.Serialization.SystemText
{
    public class OpenApiQueryConverterFactory : JsonConverterFactory
    {
        private readonly IOpenApiTypeHandler _typeHandler;

        public OpenApiQueryConverterFactory(IOpenApiTypeHandler typeHandler)
        {
            _typeHandler = typeHandler;
        }

        public override bool CanConvert(Type typeToConvert)
        {
            return typeToConvert.IsGenericType &&
                   typeToConvert.GetGenericTypeDefinition() == typeof(OpenApiQueryApplyResult<>);
        }

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            return CreateConverterOfT.Invoke(this, typeToConvert.GetGenericArguments()[0]);
        }

        private JsonConverter CreateConverter<T>()
        {
            return new OpenApiQueryApplyResultConverter<T>(_typeHandler);
        }

        private static readonly
            GenericMethodCallHelper<OpenApiQueryConverterFactory, JsonConverter>
            CreateConverterOfT =
                new GenericMethodCallHelper<OpenApiQueryConverterFactory, JsonConverter>(
                    x => x.CreateConverter<object>()
                );
    }
}
