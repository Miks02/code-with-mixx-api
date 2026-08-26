using System.Security.Claims;
using AwesomeAssertions;
using CodeWithMixx.API.Infrastructure.Security;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace CodeWithMixx.UnitTests.Infrastructure.Security;

public class UserProviderTests
{
    private readonly DefaultHttpContext _httpContext;
    private readonly UserProvider _sut;

    public UserProviderTests()
    {
        _httpContext = new DefaultHttpContext();
        _sut = new UserProvider(new HttpContextAccessor { HttpContext = _httpContext });
    }
    
    [Fact]
    public void GetUserId_WhenUserIsAuthenticated_ShouldReturnUserId()
    {
        var expectedUserId = "user-123";
        _httpContext.User = new ClaimsPrincipal(
            new ClaimsIdentity(
                new[] { new Claim(ClaimTypes.NameIdentifier, expectedUserId) },
                "TestAuthType"
            )
        );

        var actualUserId = _sut.GetUserId();

        actualUserId.Should().Be(expectedUserId);
    }
    
    [Fact]
    public void GetUserId_WhenUserIsNotAuthenticated_ShouldThrowUnauthorizedAccessException()
    {
        _httpContext.User = new ClaimsPrincipal(new ClaimsIdentity());

        Action act = () => _sut.GetUserId();

        act.Should().Throw<UnauthorizedAccessException>()
            .WithMessage("User is not authenticated");
    }

    [Fact]
    public void GetUserIpAddress_WhenIpAddressIsAvailable_ShouldReturnIpAddress()
    {
        var expectedIpAddress = "127.0.0.1";
        _httpContext.Connection.RemoteIpAddress = System.Net.IPAddress.Parse(expectedIpAddress);

        var actualIpAddress = _sut.GetUserIpAddress();

        actualIpAddress.Should().Be(expectedIpAddress);
    }
    
    [Fact]
    public void GetUserIpAddress_WhenIpAddressIsNotAvailable_ShouldReturnUnknownIp()
    {
        _httpContext.Connection.RemoteIpAddress = null;

        var actualIpAddress = _sut.GetUserIpAddress();

        actualIpAddress.Should().Be("Unknown IP");
    }
}