using System.Security.Cryptography;
using System.Text;
using AwesomeAssertions;
using CodeWithMixx.API.Domain.Entities.RefreshTokens;
using Xunit;

namespace CodeWithMixx.UnitTests.Domain.Entities.RefreshTokens;

public class RefreshTokenTests
{
    private const string UserId = "user-123";
    private const string UserIp = "192.168.1.10";
    private const string UserAgent = "Mozilla/5.0 (X11; Linux x86_64)";

    private static readonly string TokenHash = Hash("refresh-token");
    private static readonly string ReplacementHash = Hash("replacement-token");
    private static readonly DateTime ExpiresAt = new(2026, 12, 1, 12, 0, 0, DateTimeKind.Utc);
    private static readonly Guid SessionId = Guid.NewGuid();

    private readonly RefreshToken _sut;

    public RefreshTokenTests()
    {
        _sut = CreateValid();
    }

    [Fact]
    public void CreateRefreshToken_WithValidValues_ShouldMapValues()
    {
        var token = CreateValid();

        token.TokenHash.Should().Be(TokenHash);
        token.UserId.Should().Be(UserId);
        token.CreatedByIp.Should().Be(UserIp);
        token.ExpiresAt.Should().Be(ExpiresAt);
        token.UserAgent.Should().Be(UserAgent);
        token.SessionId.Should().Be(SessionId);
    }

    [Fact]
    public void CreateRefreshToken_ShouldStartNotRevoked()
    {
        _sut.RevokedAt.Should().BeNull();
        _sut.ReplacedByTokenHash.Should().BeNull();
    }

    [Fact]
    public void CreateRefreshToken_ShouldLeaveIdAndAuditFieldsUnset()
    {
        _sut.Id.Should().Be(Guid.Empty);
        _sut.CreatedAt.Should().Be(default);
        _sut.UpdatedAt.Should().BeNull();
    }

    [Fact]
    public void CreateRefreshToken_WithSha256Base64Hash_ShouldSucceed()
    {
        var hash = Hash("another-token");

        var act = () => CreateValid(tokenHash: hash);

        act.Should().NotThrow().Which.TokenHash.Should().Be(hash);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateRefreshToken_WithBlankTokenHash_ShouldThrowArgumentException(string? tokenHash)
    {
        var act = () => RefreshToken.CreateRefreshToken(tokenHash!, UserId, UserIp, ExpiresAt, UserAgent, SessionId);

        act.Should().Throw<ArgumentException>().WithParameterName("tokenHash");
    }

    [Theory]
    [InlineData(43)]
    [InlineData(45)]
    [InlineData(64)]
    public void CreateRefreshToken_WithTokenHashOfWrongLength_ShouldThrowArgumentException(int length)
    {
        var act = () => CreateValid(tokenHash: new string('a', length));

        act.Should().Throw<ArgumentException>().WithParameterName("tokenHash");
    }

    [Fact]
    public void CreateRefreshToken_With44CharNonBase64Hash_ShouldCurrentlyBeAccepted()
    {
        var hash = new string('!', 44);

        var act = () => CreateValid(tokenHash: hash);

        act.Should().NotThrow();
    }

    [Fact]
    public void CreateRefreshToken_With44WhitespaceChars_ShouldThrowArgumentException()
    {
        var act = () => CreateValid(tokenHash: new string(' ', 44));

        act.Should().Throw<ArgumentException>().WithParameterName("tokenHash");
    }

    [Fact]
    public void GenerateSessionId_ShouldReturnNonEmptyGuid()
        => RefreshToken.GenerateSessionId().Should().NotBe(Guid.Empty);

    [Fact]
    public void GenerateSessionId_ShouldReturnDistinctValues()
    {
        var ids = Enumerable.Range(0, 100).Select(_ => RefreshToken.GenerateSessionId()).ToList();

        ids.Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public void Revoke_WithoutReplacement_ShouldSetUtcRevokedAtAndLeaveReplacementNull()
    {
        var before = DateTime.UtcNow;

        _sut.Revoke();

        var after = DateTime.UtcNow;
        _sut.RevokedAt.Should().NotBeNull();
        _sut.RevokedAt!.Value.Kind.Should().Be(DateTimeKind.Utc);
        _sut.RevokedAt.Value.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
        _sut.ReplacedByTokenHash.Should().BeNull();
    }

    [Fact]
    public void Revoke_WithReplacement_ShouldSetRevokedAtAndReplacementHash()
    {
        _sut.Revoke(ReplacementHash);

        _sut.RevokedAt.Should().NotBeNull();
        _sut.ReplacedByTokenHash.Should().Be(ReplacementHash);
    }

    [Fact]
    public void Revoke_ShouldNotChangeOtherFields()
    {
        _sut.Revoke(ReplacementHash);

        _sut.TokenHash.Should().Be(TokenHash);
        _sut.UserId.Should().Be(UserId);
        _sut.CreatedByIp.Should().Be(UserIp);
        _sut.ExpiresAt.Should().Be(ExpiresAt);
        _sut.UserAgent.Should().Be(UserAgent);
        _sut.SessionId.Should().Be(SessionId);
    }

    [Fact]
    public void Revoke_WhenAlreadyRevoked_ShouldCurrentlyOverwriteRevocation()
    {
        _sut.Revoke(ReplacementHash);
        var firstRevokedAt = _sut.RevokedAt;

        _sut.Revoke();

        _sut.RevokedAt.Should().BeOnOrAfter(firstRevokedAt!.Value);
        _sut.ReplacedByTokenHash.Should().BeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData("short")]
    public void Revoke_WithMalformedReplacementHash_ShouldCurrentlyStoreItAsIs(string replacementHash)
    {
        _sut.Revoke(replacementHash);

        _sut.ReplacedByTokenHash.Should().Be(replacementHash);
    }

    private static RefreshToken CreateValid(string? tokenHash = null)
        => RefreshToken.CreateRefreshToken(tokenHash ?? TokenHash, UserId, UserIp, ExpiresAt, UserAgent, SessionId);

    private static string Hash(string value)
        => Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(value)));
}
