using System.Net;
using AwesomeAssertions;
using CodeWithMixx.API.Common.Results;
using CodeWithMixx.IntegrationTests.Common;

namespace CodeWithMixx.IntegrationTests.Features.Authenticatiion;

public class LogoutTests : IClassFixture<IntegrationTestFactory>, IAsyncLifetime
{
    private const string Url = "/api/auth/logout";

    private readonly IntegrationTestFactory _factory;
    private HttpClient _httpClient;

    public LogoutTests(IntegrationTestFactory factory)
    {
        _factory = factory;
        _httpClient = _factory.CreateClient();
    }

    public Task InitializeAsync() => _factory.ResetDatabaseAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Logout_WhenUserIsNotAuthenticated_ShouldReturnOk()
    {
        var response = await LogoutAsync();
        response.Response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.ProblemDetails.Should().BeNull();
        response.Data.Should().BeNull();
        response.Response.GetCookie("RefreshToken").Should().NotBeNull();
        response.Response.GetCookie("AccessToken").Should().NotBeNull();
        response.Response.GetCookie("Session").Should().NotBeNull();
    }
    
    [Fact]
    public async Task Logout_WhenUserIsAuthenticated_ShouldReturnOk()
    {
        _httpClient = await _factory.CreateAdminClientAsync();
        var response = await LogoutAsync();
        
        response.Response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.ProblemDetails.Should().BeNull();
        response.Data.Should().BeNull();
        response.Response.GetCookie("RefreshToken").Should().NotBeNull();
        response.Response.GetCookie("AccessToken").Should().NotBeNull();
        response.Response.GetCookie("Session").Should().NotBeNull();
    }

    private async Task<EndpointResponse<Object>> LogoutAsync()
    {
        var response = await _httpClient.PostAsync(Url, null);
        return await response.ToEndpointResponse<Object>();
    }
    
}