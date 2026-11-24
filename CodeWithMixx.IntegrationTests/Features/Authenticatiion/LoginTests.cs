using System.Net;
using System.Text;
using System.Text.Json;
using AwesomeAssertions;
using CodeWithMixx.API.Features.Authentication.Login;
using CodeWithMixx.IntegrationTests.Common;

namespace CodeWithMixx.IntegrationTests.Features.Authenticatiion;

public class LoginTests : IClassFixture<IntegrationTestFactory>, IAsyncLifetime
{
    private const string Url = "/api/auth/login";

    private readonly IntegrationTestFactory _factory;
    private readonly HttpClient _httpClient;

    public LoginTests(IntegrationTestFactory factory)
    {
        _factory = factory;
        _httpClient = _factory.CreateClient();
    }

    public Task InitializeAsync() => _factory.ResetDatabaseAsync();

    public Task DisposeAsync() => Task.CompletedTask;
    
    [Fact]
    public async Task Login_WithValidCredentials_ShouldReturnToken()
    {
        var response = await LoginAsync(TestConstants.AdminEmail, TestConstants.AdminPassword);
        
        response.Response.EnsureSuccessStatusCode();
        response.Response.Content.Should().NotBeNull();
        response.Response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Data.Should().NotBeNull();
        response.Data.Email.Should().BeEquivalentTo(TestConstants.AdminEmail);
        response.Data.FirstName.Should().BeEquivalentTo(TestConstants.AdminFirstName);
        response.Data.LastName.Should().BeEquivalentTo(TestConstants.AdminLastName);
        response.Data.PhoneNumber.Should().BeEquivalentTo(TestConstants.AdminPhoneNumber);
        response.Response.GetCookie("RefreshToken").Should().NotBeNull();
        response.Response.GetCookie("AccessToken").Should().NotBeNull();
        response.Response.GetCookie("Session").Should().NotBeNull();
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ShouldBeUnauthorized()
    {
        var user = await _factory.GetOrCreateStudentAsync();
        var response = await LoginAsync(user.Email!, "invalidpassword");
        
        response.Response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        response.Data.Should().BeNull();
        response.ProblemDetails.Should().NotBeNull();
        response.Response.Headers.Contains("Set-Cookie").Should().BeFalse();
        response.ProblemDetails.Status.Should().Be((int)HttpStatusCode.Unauthorized);
    }
    
    [Fact]
    public async Task Login_WithInvalidCredentials_ShouldBeUnauthorized()
    {
        var response = await LoginAsync("invalid@invalid.com", "123");
        
        response.Response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        response.Data.Should().BeNull();
        response.ProblemDetails.Should().NotBeNull();
        response.Response.Headers.Contains("Set-Cookie").Should().BeFalse();
        response.ProblemDetails.Status.Should().Be((int)HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_WithDeactivatedAccount_ShouldBeUnauthorized()
    {
        var user = await _factory.GetOrCreateStudentAsync();
        await _factory.DeactivateStudentAsync(user.Id);
        var response = await LoginAsync(user.Email!, TestConstants.StudentPassword);
        
        response.Response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        response.Data.Should().BeNull();
        response.ProblemDetails.Should().NotBeNull();
        response.Response.Headers.Contains("Set-Cookie").Should().BeFalse();
        response.ProblemDetails.Status.Should().Be((int)HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_WithPendingAccount_ShouldBeUnauthorized()
    {
        var user = await _factory.GetOrCreateStudentAsync();
        var response = await LoginAsync(user.Email!, TestConstants.StudentPassword);
        
        response.Response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        response.Data.Should().BeNull();
        response.ProblemDetails.Should().NotBeNull();
        response.Response.Headers.Contains("Set-Cookie").Should().BeFalse();
        response.ProblemDetails.Status.Should().Be((int)HttpStatusCode.Unauthorized);
    }
    
    

    private async Task<EndpointResponse<LoginResponse>> LoginAsync(string email, string password)
    {
        var content = new StringContent(JsonSerializer.Serialize(
            new LoginRequest {Email = email, Password = password}), Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(Url, content);
        return await response.ToEndpointResponse<LoginResponse>();
    }
}