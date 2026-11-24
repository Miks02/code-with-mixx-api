using System.Net;
using AwesomeAssertions;
using CodeWithMixx.API.Domain.Entities.RefreshTokens;
using CodeWithMixx.IntegrationTests.Common;
using Microsoft.AspNetCore.Mvc.Testing;

namespace CodeWithMixx.IntegrationTests.Features.Authenticatiion;

public class RotateTokensTests : IClassFixture<IntegrationTestFactory>, IAsyncLifetime
{
    private const string Url = "/api/auth/rotate-tokens";

    private readonly IntegrationTestFactory _factory;
    private HttpClient _httpClient;

    public RotateTokensTests(IntegrationTestFactory factory)
    {
        _factory = factory;
        _httpClient = _factory.CreateClient(new WebApplicationFactoryClientOptions { HandleCookies = false });
    }

    public Task InitializeAsync() => _factory.ResetDatabaseAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task RotateTokens_WhenRefreshTokenCookieIsMissing_ShouldReturnUnauthorized()
    {
        var response = await RotateTokensAsync();

        response.Response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        response.ProblemDetails.Should().NotBeNull();
        response.ProblemDetails.Type.Should().Be("urn:codewithmixx-api:error:Auth.MissingToken");
    }

    [Fact]
    public async Task RotateTokens_WhenRefreshTokenIsUnknown_ShouldReturnUnauthorizedAndRemoveCookies()
    {
        var response = await RotateTokensAsync(RefreshToken.CreateRawRefreshToken());

        response.Response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        response.ProblemDetails.Should().NotBeNull();
        response.ProblemDetails.Type.Should().Be("urn:codewithmixx-api:error:Auth.ExpiredToken");
        response.Response.GetCookie("RefreshToken").Should().NotBeNull();
        response.Response.GetCookie("AccessToken").Should().NotBeNull();
        response.Response.GetCookie("Session").Should().NotBeNull();
    }

    [Fact]
    public async Task RotateTokens_WhenRefreshTokenIsValid_ShouldReturnOkAndRotateTokens()
    {
        _httpClient = await _factory.CreateAdminClientAsync();
        var oldRefreshToken = _factory.CurrentRefreshToken;

        var response = await RotateTokensAsync();

        response.Response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.ProblemDetails.Should().BeNull();

        var accessCookie = response.Response.GetCookie("AccessToken");
        var refreshCookie = response.Response.GetCookie("RefreshToken");

        accessCookie.Should().NotBeNull();
        accessCookie.Value.ToString().Should().NotBeNullOrEmpty();
        refreshCookie.Should().NotBeNull();

        var newRefreshToken = Uri.UnescapeDataString(refreshCookie.Value.ToString());
        newRefreshToken.Should().NotBe(oldRefreshToken);

        var tokens = await _factory.GetAllUserRefreshTokens(TestConstants.AdminEmail);
        tokens.Should().HaveCount(2);

        var oldToken = tokens.Single(t => t.TokenHash == RefreshToken.HashRefreshToken(oldRefreshToken));
        var newToken = tokens.Single(t => t.TokenHash == RefreshToken.HashRefreshToken(newRefreshToken));

        oldToken.RevokedAt.Should().NotBeNull();
        oldToken.ReplacedByTokenHash.Should().Be(newToken.TokenHash);

        newToken.RevokedAt.Should().BeNull();
        newToken.SessionId.ToString().Should().Be(_factory.CurrentSessionId);
    }

    [Fact]
    public async Task RotateTokens_WhenRotatedRefreshTokenIsReused_ShouldReturnSecurityBreachAndRevokeAllSessions()
    {
        _httpClient = await _factory.CreateAdminClientAsync();
        await _factory.AssignRefreshTokenAsync(TestConstants.AdminEmail);

        var firstResponse = await RotateTokensAsync();
        firstResponse.Response.StatusCode.Should().Be(HttpStatusCode.OK);

        var activeTokens = await _factory.GetAllUserRefreshTokens(TestConstants.AdminEmail);
        activeTokens.Where(t => t.RevokedAt == null).Should().HaveCount(2);

        var secondResponse = await RotateTokensAsync();

        secondResponse.Response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        secondResponse.ProblemDetails.Should().NotBeNull();
        secondResponse.ProblemDetails.Type.Should().Be("urn:codewithmixx-api:error:Auth.SecurityBreach");
        secondResponse.Response.GetCookie("RefreshToken").Should().NotBeNull();
        secondResponse.Response.GetCookie("AccessToken").Should().NotBeNull();
        secondResponse.Response.GetCookie("Session").Should().NotBeNull();

        var tokens = await _factory.GetAllUserRefreshTokens(TestConstants.AdminEmail);
        tokens.Should().HaveCount(3);
        tokens.Should().OnlyContain(t => t.RevokedAt != null);
    }

    private async Task<EndpointResponse> RotateTokensAsync(string? refreshToken = null)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, Url);

        if (refreshToken is not null)
            request.SetCookie("RefreshToken", Uri.EscapeDataString(refreshToken));

        var response = await _httpClient.SendAsync(request);
        return await response.ToEndpointResponse();
    }
}
