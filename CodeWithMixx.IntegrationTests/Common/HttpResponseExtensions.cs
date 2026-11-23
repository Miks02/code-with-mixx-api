using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace CodeWithMixx.IntegrationTests.Common;

public static class HttpResponseExtensions
{
    public static async Task<EndpointResponse<T>> ToEndpointResponse<T>(this HttpResponseMessage response) where T : class
    {
        if (!response.IsSuccessStatusCode)
        {
            var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
            
            return new EndpointResponse<T>(response, null, problemDetails);
        }
        
        var raw = await response.Content.ReadAsStringAsync();
        
        var data = raw.Length > 0 ? JsonSerializer.Deserialize<T>(raw) : null;

        return new EndpointResponse<T>(response, data, null);
    }
}