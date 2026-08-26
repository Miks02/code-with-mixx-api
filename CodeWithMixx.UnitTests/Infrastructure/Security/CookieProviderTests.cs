using AwesomeAssertions;
using CodeWithMixx.API.Infrastructure.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using NSubstitute;
using Xunit;

namespace CodeWithMixx.UnitTests.Infrastructure.Security;

public class CookieProviderTests
{
    private readonly DefaultHttpContext _httpContext;
    private readonly CookieProvider _sut;

    public CookieProviderTests()
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["JwtConfig:ExpirationInMinutes"] = "15",
                ["RefreshConfig:ExpirationInDays"] = "7",
                ["CookieConfig:Domain"] = "codewithmixx.com"
            })
            .Build();

        _httpContext = new DefaultHttpContext();

        var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        httpContextAccessor.HttpContext.Returns(_httpContext);

        _sut = new CookieProvider(configuration, httpContextAccessor);
    }

    [Fact]
    public void GetRefreshTokenCookie_WhenCookieExists_ShouldReturnCookieValue()
    {
        var expectedValue = "refresh-123";
        _httpContext.Request.Headers["Cookie"] = $"RefreshToken={expectedValue}";

        var actualValue = _sut.GetRefreshTokenCookie();

        actualValue.Should().Be(expectedValue);
    }
    
    [Fact]
    public void GetAccessTokenCookie_WhenCookieExists_ShouldReturnCookieValue()
    {
        var expectedValue = "access-123";
        _httpContext.Request.Headers["Cookie"] = $"AccessToken={expectedValue}";

        var actualValue = _sut.GetAccessTokenCookie();

        actualValue.Should().Be(expectedValue);
    }
    
    [Fact]
    public void GetAccessTokenCookie_WhenCookieMissing_ReturnsNull()
    {
        var result = _sut.GetAccessTokenCookie();

        result.Should().BeNull();
    }

    [Fact]
    public void GetRefreshTokenCookie_WhenCookieExists_ReturnsValue()
    {
        _httpContext.Request.Headers["Cookie"] = "RefreshToken=xyz789";

        var result = _sut.GetRefreshTokenCookie();

        result.Should().Be("xyz789");
    }
    
    [Fact]
    public void SetAccessTokenCookie_AppendsCookieWithCorrectAttributes()
    {
        _sut.SetAccessTokenCookie("my-access-token");

        var setCookieHeader = _httpContext.Response.Headers.SetCookie.ToString();

        setCookieHeader.Should().Contain("AccessToken=my-access-token");
        setCookieHeader.Should().Contain("domain=codewithmixx.com");
        setCookieHeader.Should().Contain("path=/");
        setCookieHeader.Should().Contain("httponly");
        setCookieHeader.Should().Contain("secure");
        setCookieHeader.Should().Contain("samesite=lax");
    }

    [Fact]
    public void SetRefreshTokenCookie_AppendsCookieWithLongerExpiration()
    {
        _sut.SetRefreshTokenCookie("my-refresh-token");

        var setCookieHeader = _httpContext.Response.Headers.SetCookie.ToString();

        setCookieHeader.Should().Contain("RefreshToken=my-refresh-token");
    }

    [Fact]
    public void RemoveAuthCookies_DeletesBothCookies()
    {
        _sut.RemoveAuthCookies();

        var setCookieHeaders = _httpContext.Response.Headers.SetCookie;

        setCookieHeaders.Should().Contain(h => h!.StartsWith("AccessToken="));
        setCookieHeaders.Should().Contain(h => h!.StartsWith("RefreshToken="));
    }

}