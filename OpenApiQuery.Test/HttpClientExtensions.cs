using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace OpenApiQuery.Test
{
    public static class HttpClientExtensions
    {
        private static readonly JsonOptions Options = new JsonOptions();

        public static async Task<OpenApiQueryApplyResult<T>> GetQueryAsync<T>(this HttpClient client, string requestUri,
            CancellationToken cancellationToken = default)
        {
            using var response = await client.GetAsync(requestUri, cancellationToken);
            response.EnsureSuccessStatusCode();

            await using var json = await response.Content.ReadAsStreamAsync();
            return await JsonSerializer.DeserializeAsync<OpenApiQueryApplyResult<T>>(json,
                Options.JsonSerializerOptions,
                cancellationToken);
        }
    }
}