using System.IdentityModel.Tokens.Jwt;
using AwesomeAssertions;
using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Domain.Entities.Users;
using CodeWithMixx.API.Infrastructure.Persistence;
using CodeWithMixx.API.Infrastructure.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NSubstitute;
using Xunit;

namespace CodeWithMixx.UnitTests.Infrastructure.Security;

public class TokenServiceTests
{
    private readonly IUserProvider _userProvider;
    private readonly AppDbContext _context;
    private readonly UserManager<User> _userManager;
    private readonly TokenService _sut;

    public TokenServiceTests()
    {
        _userProvider = Substitute.For<IUserProvider>();
        _userProvider.GetUserIpAddress().Returns("127.0.0.1");

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["JwtConfig:Token"] = "extra-long-secret-key-for-testing-purposes-1234567890",
                ["JwtConfig:Issuer"] = "CodeWithMixx.UnitTests",
                ["JwtConfig:Audience"] = "CodeWithMixx.UnitTests",
                ["JwtConfig:ExpirationInMinutes"] = "15",
                ["RefreshConfig:ExpirationInDays"] = "7"
            })
            .Build();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);

        var userStore = Substitute.For<IUserStore<User>>();
        _userManager = Substitute.For<UserManager<User>>(userStore, null, null, null, null, null, null, null, null);

        _userManager.GetRolesAsync(Arg.Any<User>()).Returns(new List<string>());

        _sut = new TokenService(configuration, _context, _userManager, _userProvider);
    }

    [Fact]
    public async Task AssignAuthTokens_ShouldReturnValidTokens()
    {
        var user = User.CreateUser("Marko", "Markovic", "marko@test.com", "123456789");

        var result = await _sut.AssignAuthTokens(user);

        result.Should().NotBeNull();
        result.AccessToken.Should().NotBeNullOrEmpty();
        result.RefreshToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task AssignAuthTokens_AccessToken_ContainsExpectedClaims()
    {
        var user = User.CreateUser("Marko", "Markovic", "marko@test.com", "123456789");
        _userManager.GetRolesAsync(user).Returns(new List<string> { "Admin", "User" });

        var result = await _sut.AssignAuthTokens(user);

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(result.AccessToken);

        jwt.Issuer.Should().Be("CodeWithMixx.UnitTests");
        jwt.Audiences.Should().Contain("CodeWithMixx.UnitTests");
        jwt.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Email && c.Value == "marko@test.com");
        jwt.Claims.Should().Contain(c => c.Type == "role" && c.Value == "Admin");
        jwt.Claims.Should().Contain(c => c.Type == "role" && c.Value == "User");
        jwt.ValidTo.Should().BeCloseTo(DateTime.UtcNow.AddMinutes(15), TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task AssignAuthTokens_PersistsRefreshTokenInDatabase()
    {
        var user = User.CreateUser("Marko", "Markovic", "marko@test.com", "123456789");

        var result = await _sut.AssignAuthTokens(user);

        var storedToken = await _context.RefreshTokens
            .SingleOrDefaultAsync(rt => rt.UserId == user.Id);

        storedToken.Should().NotBeNull();
        storedToken!.TokenHash.Should().Be(_sut.HashRefreshToken(result.RefreshToken));
        storedToken.ExpiresAt.Should().BeCloseTo(DateTime.UtcNow.AddDays(7), TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task AssignAuthTokens_CallsUserProviderForIpAddress()
    {
        var user = User.CreateUser("Marko", "Markovic", "marko@test.com", "123456789");

        await _sut.AssignAuthTokens(user);

        _userProvider.Received(1).GetUserIpAddress();
    }

    [Fact]
    public void HashRefreshToken_SameInput_ProducesSameHash()
    {
        var hash1 = _sut.HashRefreshToken("some-raw-token");
        var hash2 = _sut.HashRefreshToken("some-raw-token");

        hash1.Should().Be(hash2);
    }

    [Fact]
    public void HashRefreshToken_DifferentInputs_ProduceDifferentHashes()
    {
        var hash1 = _sut.HashRefreshToken("token-one");
        var hash2 = _sut.HashRefreshToken("token-two");

        hash1.Should().NotBe(hash2);
    }
}