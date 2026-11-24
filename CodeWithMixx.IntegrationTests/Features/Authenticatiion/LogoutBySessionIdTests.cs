using System.Net;
using System.Text;
using System.Text.Json;
using AwesomeAssertions;
using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Features.Authentication.Login;
using CodeWithMixx.API.Features.Authentication.LogoutBySessionId;
using CodeWithMixx.IntegrationTests.Common;

namespace CodeWithMixx.IntegrationTests.Features.Authenticatiion;

public class LogoutBySessionIdTests : IClassFixture<IntegrationTestFactory>, IAsyncLifetime
{
    private const string Url = "/api/auth/logout-session";

    private readonly IntegrationTestFactory _factory;
    private HttpClient _httpClient;

    public LogoutBySessionIdTests(IntegrationTestFactory factory)
    {
        _factory = factory;
        _httpClient = _factory.CreateClient();
    }

    public Task InitializeAsync() => _factory.ResetDatabaseAsync();

    public Task DisposeAsync() => Task.CompletedTask;
    
    [Fact]
    public async Task LogoutBySessionId_WhenUserIsNotAuthenticated_ShouldReturnUnauthorized()
    {
        var response = await LogoutBySessionId("invalid-session-id");

        response.Response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
    
    [Fact]
    public async Task LogoutBySessionId_WhenUserIsAuthenticated_ShouldReturnOkAndRevokeToken()
    {
        _httpClient = await _factory.CreateAdminClientAsync();
        var session = _factory.CurrentSessionId;
        var response = await LogoutBySessionId(session);

        response.Response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.ProblemDetails.Should().BeNull();
        
        var token = await _factory.GetRefreshTokenBySessionId(session);
        
        token.Should().NotBeNull();
        token.RevokedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task LogoutBySessionId_WhenProvidedSessionIdIsCurrentSession_ShouldRemoveCookies()
    {
        _httpClient = await _factory.CreateAdminClientAsync();
        var session = _factory.CurrentSessionId;
        var response = await LogoutBySessionId(session);

        response.Response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.ProblemDetails.Should().BeNull();
        response.Response.GetCookie("RefreshToken").Should().NotBeNull();
        response.Response.GetCookie("AccessToken").Should().NotBeNull();
        response.Response.GetCookie("Session").Should().NotBeNull();

    }

    private async Task<EndpointResponse> LogoutBySessionId(string sessionId)
    {
        var content = new StringContent(JsonSerializer.Serialize(
            new LogoutBySessionIdRequest { SessionId = sessionId }), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync(Url, content);
        
        return await response.ToEndpointResponse();
    }
}