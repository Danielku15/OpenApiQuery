using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenApiQuery.Parsing;
using OpenApiQuery.Serialization.SystemText;

namespace OpenApiQuery.Test
{
    public static class HttpClientExtensions
    {
        private static readonly DefaultOpenApiTypeHandler TypeHandler = new DefaultOpenApiTypeHandler();

        private static readonly JsonOptions Options = new JsonOptions
        {
            JsonSerializerOptions =
            {
                Converters =
                {
                    new OpenApiQueryDeltaConverterFactory(TypeHandler),
                    new OpenApiQueryResultConverterFactory(TypeHandler),
                    new OpenApiQuerySingleResultConverterFactory(TypeHandler)
                }
            }
        };

        public static async Task<OpenApiQueryResult<T>> GetQueryAsync<T>(
            this HttpClient client,
            string requestUri,
            CancellationToken cancellationToken = default)
        {
            using var response = await client.GetAsync(requestUri, cancellationToken);
            response.EnsureSuccessStatusCode();

            await using var json = await response.Content.ReadAsStreamAsync();
            try
            {
                return await JsonSerializer.DeserializeAsync<OpenApiQueryResult<T>>(json,
                    Options.JsonSerializerOptions,
                    cancellationToken);
            }
            catch (JsonException e)
            {
                var jsonText = await response.Content.ReadAsStringAsync();
                Assert.Fail($"Failed to deserialize JSON: '{jsonText}', {e}");
                throw;
            }
        }
        public static async Task<T> GetSingleAsync<T>(
            this HttpClient client,
            string requestUri,
            CancellationToken cancellationToken = default)
        {
            using var response = await client.GetAsync(requestUri, cancellationToken);
            response.EnsureSuccessStatusCode();

            await using var json = await response.Content.ReadAsStreamAsync();
            try
            {
                return await JsonSerializer.DeserializeAsync<T>(json,
                    Options.JsonSerializerOptions,
                    cancellationToken);
            }
            catch (JsonException e)
            {
                var jsonText = await response.Content.ReadAsStringAsync();
                Assert.Fail($"Failed to deserialize JSON: '{jsonText}', {e}");
                throw;
            }
        }
    }
}
