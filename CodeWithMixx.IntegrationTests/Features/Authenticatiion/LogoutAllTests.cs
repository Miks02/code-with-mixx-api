using System.Net;
using AwesomeAssertions;
using CodeWithMixx.IntegrationTests.Common;

namespace CodeWithMixx.IntegrationTests.Features.Authenticatiion;

public class LogoutAllTests : IClassFixture<IntegrationTestFactory>, IAsyncLifetime
{
    private const string Url = "/api/auth/logout-all";

    private readonly IntegrationTestFactory _factory;
    private HttpClient _httpClient;

    public LogoutAllTests(IntegrationTestFactory factory)
    {
        _factory = factory;
        _httpClient = _factory.CreateClient();
    }

    public Task InitializeAsync() => _factory.ResetDatabaseAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task LogoutAll_WhenUserIsNotAuthenticated_ShouldReturnUnauthorized()
    {
        var response = await LogoutAllAsync();
        
        response.Response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task LogoutAll_WhenUserIsAuthenticated_ShouldReturnOk()
    {
        _httpClient = await _factory.CreateAdminClientAsync();
        var response = await LogoutAllAsync();
        
        response.ProblemDetails.Should().BeNull();
        response.Response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Response.GetCookie("RefreshToken").Should().NotBeNull();
        response.Response.GetCookie("AccessToken").Should().NotBeNull();
        response.Response.GetCookie("Session").Should().NotBeNull();
    }
    
    [Fact]
    public async Task LogoutAll_WhenUserIsAuthenticated_ShouldRevokeTokens()
    {
        _httpClient = await _factory.CreateAdminClientAsync();

        await _factory.AssignRefreshTokenAsync(TestConstants.AdminEmail);
        await _factory.AssignRefreshTokenAsync(TestConstants.AdminEmail);
        await _factory.AssignRefreshTokenAsync(TestConstants.AdminEmail);
        
        var tokens = await _factory.GetAllUserRefreshTokens(TestConstants.AdminEmail);

        tokens.Should().HaveCount(4);
        
        await LogoutAllAsync();
        
        tokens = await _factory.GetAllUserRefreshTokens(TestConstants.AdminEmail);
        tokens.Should().HaveCount(4);
        tokens.Where(rt => rt.RevokedAt != null).Should().HaveCount(4);
    }
    
    private async Task<EndpointResponse> LogoutAllAsync()
    {
        var response = await _httpClient.PostAsync(Url, null);
        return await response.ToEndpointResponse();
    }
    
}