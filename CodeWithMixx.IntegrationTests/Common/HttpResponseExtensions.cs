using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;

namespace CodeWithMixx.IntegrationTests.Common;

public static class HttpResponseExtensions
{

    public static async Task<EndpointResponse<T>> ToEndpointResponse<T>(this HttpResponseMessage response) where T : class
    {
        var raw = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            var problemDetails = raw.Length > 0 ? JsonSerializer.Deserialize<ProblemDetails>(raw, JsonOptions) : null;

            return new EndpointResponse<T>(response, null, problemDetails);
        }

        var data = raw.Length > 0 ? JsonSerializer.Deserialize<T>(raw, JsonOptions) : null;

        return new EndpointResponse<T>(response, data, null);
    }
    
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };
}
