using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using OpenApiQuery.Parsing;
using JsonException = System.Text.Json.JsonException;

namespace OpenApiQuery.Test
{
    public static class HttpClientExtensions
    {
        private static readonly DefaultOpenApiTypeHandler TypeHandler = new DefaultOpenApiTypeHandler();

        public static async Task<OpenApiQueryResult<T>> GetQueryAsync<T>(
            this HttpClient client,
            string requestUri,
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
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<OpenApiQueryResult<T>>(json);
            }
            catch (JsonException e)
            {
                var jsonText = await response.Content.ReadAsStringAsync();
                Assert.Fail($"Failed to deserialize JSON: '{jsonText}', {e}");
                throw;
            }
        }
        public static async Task<T> GetSingleQueryAsync<T>(
            this HttpClient client,
            string requestUri,
            CancellationToken cancellationToken = default)
        {
            using var response = await client.GetAsync(requestUri, cancellationToken);
            response.EnsureSuccessStatusCode();

            try
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<T>(json);
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

            try
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<T>(json);
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
