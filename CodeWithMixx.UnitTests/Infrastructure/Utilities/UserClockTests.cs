using AwesomeAssertions;
using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Infrastructure.Utility;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Xunit;

namespace CodeWithMixx.UnitTests.Infrastructure.Utilities;

public class UserClockTests
{
    private const string TimeZoneHeader = "TimeZoneId";
    private const string FallbackZoneId = "Europe/Belgrade";

    private readonly DefaultHttpContext _httpContext;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly TimeProvider _timeProvider;
    private readonly UserClock _sut;

    public UserClockTests()
    {
        _timeProvider = Substitute.For<TimeProvider>();
        _httpContext = new DefaultHttpContext();

        _httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        _httpContextAccessor.HttpContext.Returns(_httpContext);

        _sut = new UserClock(_httpContextAccessor, _timeProvider);
    }

    [Fact]
    public void TimeZone_WhenHeaderContainsValidTimeZoneId_ShouldReturnClientTimeZone()
    {
        _httpContext.Request.Headers[TimeZoneHeader] = "America/New_York";

        var timeZone = _sut.TimeZone;

        timeZone.Id.Should().Be("America/New_York");
    }

    [Fact]
    public void TimeZone_WhenHeaderIsMissing_ShouldFallBackToBelgrade()
    {
        var timeZone = _sut.TimeZone;

        timeZone.Id.Should().Be(FallbackZoneId);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("Not/AZone")]
    public void TimeZone_WhenHeaderIsBlankOrUnknown_ShouldFallBackToBelgrade(string headerValue)
    {
        _httpContext.Request.Headers[TimeZoneHeader] = headerValue;

        var timeZone = _sut.TimeZone;

        timeZone.Id.Should().Be(FallbackZoneId);
    }

    [Fact]
    public void TimeZone_WhenThereIsNoHttpContext_ShouldFallBackToBelgrade()
    {
        var accessor = Substitute.For<IHttpContextAccessor>();
        accessor.HttpContext.Returns((HttpContext?)null);
        var sut = new UserClock(accessor, _timeProvider);

        var timeZone = sut.TimeZone;

        timeZone.Id.Should().Be(FallbackZoneId);
    }

    [Fact]
    public void TimeZone_WhenHeaderChangesAfterFirstAccess_ShouldKeepFirstResolvedZone()
    {
        _httpContext.Request.Headers[TimeZoneHeader] = "America/New_York";
        var firstTimeZone = _sut.TimeZone;

        _httpContext.Request.Headers[TimeZoneHeader] = "Asia/Tokyo";
        var secondTimeZone = _sut.TimeZone;

        secondTimeZone.Should().BeSameAs(firstTimeZone);
        secondTimeZone.Id.Should().Be("America/New_York");
    }

    [Fact]
    public void LocalNow_InWinter_ShouldConvertUtcToBelgradeStandardTime()
    {
        _timeProvider.GetUtcNow().Returns(new DateTimeOffset(2026, 1, 15, 12, 0, 0, TimeSpan.Zero));

        var localNow = _sut.LocalNow;

        localNow.Should().Be(new DateTimeOffset(2026, 1, 15, 13, 0, 0, TimeSpan.FromHours(1)));
        localNow.Offset.Should().Be(TimeSpan.FromHours(1));
    }

    [Fact]
    public void LocalNow_InSummer_ShouldConvertUtcToBelgradeDaylightTime()
    {
        _timeProvider.GetUtcNow().Returns(new DateTimeOffset(2026, 7, 15, 12, 0, 0, TimeSpan.Zero));

        var localNow = _sut.LocalNow;

        localNow.DateTime.Should().Be(new DateTime(2026, 7, 15, 14, 0, 0));
        localNow.Offset.Should().Be(TimeSpan.FromHours(2));
    }

    [Fact]
    public void Today_WhenUtcIsBeforeMidnightButBelgradeIsAfter_ShouldReturnBelgradeDate()
    {
        _timeProvider.GetUtcNow().Returns(new DateTimeOffset(2026, 3, 10, 23, 30, 0, TimeSpan.Zero));

        var today = _sut.Today;

        today.Should().Be(new DateOnly(2026, 3, 11));
    }

    [Fact]
    public void Today_WhenUtcIsAfterMidnightButClientZoneIsBefore_ShouldReturnClientDate()
    {
        _httpContext.Request.Headers[TimeZoneHeader] = "America/New_York";
        _timeProvider.GetUtcNow().Returns(new DateTimeOffset(2026, 3, 11, 3, 0, 0, TimeSpan.Zero));

        var today = _sut.Today;

        today.Should().Be(new DateOnly(2026, 3, 10));
    }

    [Fact]
    public void AddUtility_ShouldRegisterUserClockAsScopedIUserClock()
    {
        var services = new ServiceCollection();
        services.AddSingleton(_httpContextAccessor);
        services.AddSingleton(_timeProvider);
        services.AddUtility();
        using var provider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });

        using var firstScope = provider.CreateScope();
        using var secondScope = provider.CreateScope();
        var first = firstScope.ServiceProvider.GetRequiredService<IUserClock>();
        var sameScope = firstScope.ServiceProvider.GetRequiredService<IUserClock>();
        var otherScope = secondScope.ServiceProvider.GetRequiredService<IUserClock>();

        first.Should().BeOfType<UserClock>();
        sameScope.Should().BeSameAs(first);
        otherScope.Should().NotBeSameAs(first);
    }
}
