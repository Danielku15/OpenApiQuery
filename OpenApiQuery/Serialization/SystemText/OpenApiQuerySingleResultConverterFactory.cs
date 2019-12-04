using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using OpenApiQuery.Parsing;
using OpenApiQuery.Utils;

namespace OpenApiQuery.Serialization.SystemText
{
    public class OpenApiQuerySingleResultConverterFactory : JsonConverterFactory
    {
        private readonly IOpenApiTypeHandler _typeHandler;

        public OpenApiQuerySingleResultConverterFactory(IOpenApiTypeHandler typeHandler)
        {
            _typeHandler = typeHandler;
        }

        public override bool CanConvert(Type typeToConvert)
        {
            return typeToConvert.IsGenericType &&
                   typeToConvert.GetGenericTypeDefinition() == typeof(OpenApiQuerySingleResult<>);
        }

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            return CreateConverterOfT.Invoke(this, typeToConvert.GetGenericArguments()[0]);
        }

        private JsonConverter CreateConverter<T>()
        {
            return new OpenApiQuerySingleResultConverter<T>(_typeHandler);
        }

        private static readonly
            GenericMethodCallHelper<OpenApiQuerySingleResultConverterFactory, JsonConverter>
            CreateConverterOfT =
                new GenericMethodCallHelper<OpenApiQuerySingleResultConverterFactory, JsonConverter>(
                    x => x.CreateConverter<object>()
                );
    }
}
