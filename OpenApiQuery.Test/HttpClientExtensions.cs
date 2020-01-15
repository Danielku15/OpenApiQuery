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

        private static readonly JsonOptions Options = BuildOptions(TypeHandler);

        private static JsonOptions BuildOptions(IOpenApiTypeHandler handler)
        {
            return new JsonOptions
            {
                JsonSerializerOptions =
                {
                    Converters =
                    {
                        new OpenApiQueryDeltaConverterFactory(handler),
                        new OpenApiQueryResultConverterFactory(handler),
                        new OpenApiQuerySingleResultConverterFactory(handler)
                    }
                }
            };
        }

        public static async Task<OpenApiQueryResult<T>> GetQueryAsync<T>(
            this HttpClient client,
            string requestUri,
            IOpenApiTypeHandler typeHandler = null,
            CancellationToken cancellationToken = default)
        {
            using var response = await client.GetAsync(requestUri, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var jsonText = await response.Content.ReadAsStringAsync();
                Assert.Fail($"Request failed with status code {response.StatusCode} and response {jsonText}");
            }

            try
            {
                var options = (typeHandler != null && typeHandler != TypeHandler) ? BuildOptions(typeHandler) : Options;
                await using var json = await response.Content.ReadAsStreamAsync();
                return await JsonSerializer.DeserializeAsync<OpenApiQueryResult<T>>(json,
                    options.JsonSerializerOptions,
                    cancellationToken);
            }
            catch (JsonException e)
            {
                var jsonText = await response.Content.ReadAsStringAsync();
                Assert.Fail($"Failed to deserialize JSON: '{jsonText}', {e}");
                throw;
            }
        }
        public static async Task<OpenApiQuerySingleResult<T>> GetSingleQueryAsync<T>(
            this HttpClient client,
            string requestUri,
            IOpenApiTypeHandler typeHandler = null,
            CancellationToken cancellationToken = default)
        {
            using var response = await client.GetAsync(requestUri, cancellationToken);
            response.EnsureSuccessStatusCode();

            await using var json = await response.Content.ReadAsStreamAsync();
            try
            {
                var options = (typeHandler != null && typeHandler != TypeHandler) ? BuildOptions(typeHandler) : Options;
                return await JsonSerializer.DeserializeAsync<OpenApiQuerySingleResult<T>>(json,
                    options.JsonSerializerOptions,
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
            IOpenApiTypeHandler typeHandler = null,
            CancellationToken cancellationToken = default)
        {
            using var response = await client.GetAsync(requestUri, cancellationToken);
            response.EnsureSuccessStatusCode();

            await using var json = await response.Content.ReadAsStreamAsync();
            try
            {
                var options = (typeHandler != null && typeHandler != TypeHandler) ? BuildOptions(typeHandler) : Options;
                return await JsonSerializer.DeserializeAsync<T>(json,
                    options.JsonSerializerOptions,
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
