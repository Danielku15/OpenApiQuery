using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using OpenApiQuery.Parsing;
using OpenApiQuery.Utils;

namespace OpenApiQuery.Serialization.SystemText
{
    public class OpenApiQueryResultConverterFactory : JsonConverterFactory
    {
        private readonly IOpenApiTypeHandler _typeHandler;

        public OpenApiQueryResultConverterFactory(IOpenApiTypeHandler typeHandler)
        {
            _typeHandler = typeHandler;
        }

        public override bool CanConvert(Type typeToConvert)
        {
            return typeToConvert.IsGenericType &&
                   typeToConvert.GetGenericTypeDefinition() == typeof(Multiple<>);
        }

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            return CreateConverterOfT.Invoke(this, typeToConvert.GetGenericArguments()[0]);
        }

        private JsonConverter CreateConverter<T>()
        {
            return new OpenApiQueryResultConverter<T>(_typeHandler);
        }

        private static readonly
            GenericMethodCallHelper<OpenApiQueryResultConverterFactory, JsonConverter>
            CreateConverterOfT =
                new GenericMethodCallHelper<OpenApiQueryResultConverterFactory, JsonConverter>(
                    x => x.CreateConverter<object>()
                );
    }
}
