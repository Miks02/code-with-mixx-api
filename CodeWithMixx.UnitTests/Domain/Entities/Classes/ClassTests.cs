using AwesomeAssertions;
using CodeWithMixx.API.Common.Result;
using CodeWithMixx.API.Domain.Entities.Classes;
using CodeWithMixx.API.Domain.Entities.Subjects;
using Xunit;

namespace CodeWithMixx.UnitTests.Domain.Entities.Classes;

public class ClassTests
{
    private const int SubjectId = 3;
    private const decimal Price = ClassConstants.DefaultPrice;

    private static readonly DateTime StartsAt = new(2026, 11, 20, 16, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime EndsAt = StartsAt.AddMinutes(ClassConstants.MaxDurationInMinutes);
    private static readonly DateOnly ReservedAt = new(2026, 11, 15);

    private readonly Class _sut;

    public ClassTests()
    {
        _sut = CreateValid().Payload!;
    }

    [Fact]
    public void Create_WithValidValues_ShouldSucceedAndMapValues()
    {
        var result = CreateValid();

        result.IsSuccess.Should().BeTrue();
        var cls = result.Payload!;
        cls.SubjectId.Should().Be(SubjectId);
        cls.Price.Should().Be(Price);
        cls.StartsAt.Should().Be(StartsAt);
        cls.EndsAt.Should().Be(EndsAt);
        cls.ReservedAt.Should().Be(ReservedAt);
    }

    [Fact]
    public void Create_ShouldStartNotDeletedWithUnsetIdsAndAuditFields()
    {
        _sut.Id.Should().Be(0);
        _sut.ReservationId.Should().Be(0);
        _sut.CreatedAt.Should().Be(default);
        _sut.UpdatedAt.Should().BeNull();
        _sut.IsDeleted.Should().BeFalse();
        _sut.DeletedAt.Should().BeNull();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_WithNonPositiveSubjectId_ShouldReturnSubjectNotFound(int subjectId)
        => AssertFails(CreateValid(subjectId: subjectId), SubjectError.NotFound(subjectId));

    [Theory]
    [InlineData(0)]
    [InlineData(-0.01)]
    [InlineData(-1300)]
    public void Create_WithNonPositivePrice_ShouldReturnInvalidPrice(decimal price)
        => AssertFails(CreateValid(price: price), ClassError.InvalidPrice(price));

    [Fact]
    public void Create_WithSmallestPositivePrice_ShouldSucceed()
        => CreateValid(price: 0.01m).IsSuccess.Should().BeTrue();

    [Fact]
    public void Create_WithEndsAtEqualToStartsAt_ShouldReturnInvalidSchedule()
        => AssertFails(CreateValid(endsAt: StartsAt), ClassError.InvalidSchedule(StartsAt, StartsAt));

    [Fact]
    public void Create_WithEndsAtBeforeStartsAt_ShouldReturnInvalidSchedule()
    {
        var endsAt = StartsAt.AddMinutes(-30);

        AssertFails(CreateValid(endsAt: endsAt), ClassError.InvalidSchedule(StartsAt, endsAt));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(30)]
    [InlineData(ClassConstants.MaxDurationInMinutes)]
    public void Create_WithDurationWithinLimit_ShouldSucceed(int minutes)
        => CreateValid(endsAt: StartsAt.AddMinutes(minutes)).IsSuccess.Should().BeTrue();

    [Fact]
    public void Create_WithDurationOneSecondOverLimit_ShouldReturnInvalidDuration()
        => AssertFails(CreateValid(endsAt: EndsAt.AddSeconds(1)), ClassError.InvalidDuration());

    [Fact]
    public void Create_WithDurationOverLimit_ShouldReturnInvalidDuration()
        => AssertFails(CreateValid(endsAt: StartsAt.AddMinutes(ClassConstants.MaxDurationInMinutes + 1)), ClassError.InvalidDuration());

    [Fact]
    public void Create_SpanningMidnight_ShouldSucceed()
    {
        var startsAt = new DateTime(2026, 11, 20, 23, 30, 0, DateTimeKind.Utc);

        CreateValid(startsAt: startsAt, endsAt: startsAt.AddMinutes(ClassConstants.MaxDurationInMinutes)).IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Create_WithReservedAtOnClassDay_ShouldSucceed()
        => CreateValid(reservedAt: DateOnly.FromDateTime(StartsAt)).IsSuccess.Should().BeTrue();

    [Fact]
    public void Create_WithReservedAtAfterClassDay_ShouldReturnInvalidReservationDate()
        => AssertFails(CreateValid(reservedAt: DateOnly.FromDateTime(StartsAt).AddDays(1)), ClassError.InvalidReservationDate());

    [Fact]
    public void Create_WithMultipleInvalidValues_ShouldReportSubjectFirst()
    {
        var result = Class.Create(0, 0, StartsAt, StartsAt, DateOnly.FromDateTime(StartsAt).AddDays(1));

        AssertFails(result, SubjectError.NotFound(0));
    }

    [Fact]
    public void Create_WithInvalidPriceAndSchedule_ShouldReportPriceFirst()
        => AssertFails(CreateValid(price: 0, endsAt: StartsAt), ClassError.InvalidPrice(0));

    [Fact]
    public void Create_WithInvalidScheduleAndReservationDate_ShouldReportScheduleFirst()
    {
        var reservedAt = DateOnly.FromDateTime(StartsAt).AddDays(1);

        AssertFails(CreateValid(endsAt: StartsAt, reservedAt: reservedAt), ClassError.InvalidSchedule(StartsAt, StartsAt));
    }

    [Fact]
    public void Create_WithDefaultReservedAt_ShouldCurrentlyBeAccepted()
        => CreateValid(reservedAt: default).IsSuccess.Should().BeTrue();

    [Fact]
    public void Update_WithValidValues_ShouldSucceedAndReplaceValues()
    {
        var startsAt = StartsAt.AddDays(7);
        var endsAt = startsAt.AddMinutes(30);
        var reservedAt = ReservedAt.AddDays(1);

        var result = _sut.Update(9, 1500m, startsAt, endsAt, reservedAt);

        result.IsSuccess.Should().BeTrue();
        result.Payload.Should().BeSameAs(_sut);
        _sut.SubjectId.Should().Be(9);
        _sut.Price.Should().Be(1500m);
        _sut.StartsAt.Should().Be(startsAt);
        _sut.EndsAt.Should().Be(endsAt);
        _sut.ReservedAt.Should().Be(reservedAt);
    }

    [Fact]
    public void Update_WithSameValues_ShouldSucceed()
        => _sut.Update(SubjectId, Price, StartsAt, EndsAt, ReservedAt).IsSuccess.Should().BeTrue();

    [Fact]
    public void Update_WithInvalidSubjectId_ShouldReturnSubjectNotFoundAndKeepValues()
    {
        var result = _sut.Update(0, 1500m, StartsAt, EndsAt, ReservedAt);

        AssertFails(result, SubjectError.NotFound(0));
        AssertUnchanged();
    }

    [Fact]
    public void Update_WithInvalidPrice_ShouldReturnInvalidPriceAndKeepValues()
    {
        var result = _sut.Update(9, 0, StartsAt, EndsAt, ReservedAt);

        AssertFails(result, ClassError.InvalidPrice(0));
        AssertUnchanged();
    }

    [Fact]
    public void Update_WithInvalidSchedule_ShouldReturnInvalidScheduleAndKeepValues()
    {
        var result = _sut.Update(SubjectId, Price, EndsAt, StartsAt, ReservedAt);

        AssertFails(result, ClassError.InvalidSchedule(EndsAt, StartsAt));
        AssertUnchanged();
    }

    [Fact]
    public void Update_WithDurationOverLimit_ShouldReturnInvalidDurationAndKeepValues()
    {
        var result = _sut.Update(SubjectId, Price, StartsAt, StartsAt.AddMinutes(ClassConstants.MaxDurationInMinutes + 1), ReservedAt);

        AssertFails(result, ClassError.InvalidDuration());
        AssertUnchanged();
    }

    [Fact]
    public void Update_WithReservedAtAfterClassDay_ShouldReturnInvalidReservationDateAndKeepValues()
    {
        var result = _sut.Update(SubjectId, Price, StartsAt, EndsAt, DateOnly.FromDateTime(StartsAt).AddDays(1));

        AssertFails(result, ClassError.InvalidReservationDate());
        AssertUnchanged();
    }

    [Fact]
    public void Update_WhenDeleted_ShouldCurrentlySucceed()
    {
        _sut.Delete();

        _sut.Update(SubjectId, 1500m, StartsAt, EndsAt, ReservedAt).IsSuccess.Should().BeTrue();
        _sut.Price.Should().Be(1500m);
    }

    [Fact]
    public void Update_ShouldNotChangeDeletionState()
    {
        _sut.Delete();
        var deletedAt = _sut.DeletedAt;

        _sut.Update(SubjectId, 1500m, StartsAt, EndsAt, ReservedAt);

        _sut.IsDeleted.Should().BeTrue();
        _sut.DeletedAt.Should().Be(deletedAt);
    }

    [Fact]
    public void Delete_WhenNotDeleted_ShouldSucceedWithUtcTimestamp()
    {
        var before = DateTime.UtcNow;

        var result = _sut.Delete();

        var after = DateTime.UtcNow;
        result.IsSuccess.Should().BeTrue();
        _sut.IsDeleted.Should().BeTrue();
        _sut.DeletedAt.Should().NotBeNull();
        _sut.DeletedAt!.Value.Kind.Should().Be(DateTimeKind.Utc);
        _sut.DeletedAt.Value.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
    }

    [Fact]
    public void Delete_ShouldNotChangeOtherValues()
    {
        _sut.Delete();

        AssertUnchanged();
    }

    [Fact]
    public void Delete_WhenAlreadyDeleted_ShouldReturnAlreadyDeletedAndKeepOriginalDeletedAt()
    {
        _sut.Delete();
        var firstDeletedAt = _sut.DeletedAt;

        var result = _sut.Delete();

        AssertFails(result, ClassError.AlreadyDeleted(_sut.Id));
        _sut.DeletedAt.Should().Be(firstDeletedAt);
    }

    [Fact]
    public void Restore_WhenDeleted_ShouldSucceedAndClearDeletion()
    {
        _sut.Delete();

        var result = _sut.Restore();

        result.IsSuccess.Should().BeTrue();
        _sut.IsDeleted.Should().BeFalse();
        _sut.DeletedAt.Should().BeNull();
    }

    [Fact]
    public void Restore_WhenNotDeleted_ShouldReturnNotArchived()
    {
        var result = _sut.Restore();

        AssertFails(result, ClassError.NotArchived(_sut.Id));
        _sut.IsDeleted.Should().BeFalse();
        _sut.DeletedAt.Should().BeNull();
    }

    [Fact]
    public void Delete_AfterRestore_ShouldSucceedWithNewDeletedAt()
    {
        _sut.Delete();
        var firstDeletedAt = _sut.DeletedAt;
        _sut.Restore();

        var result = _sut.Delete();

        result.IsSuccess.Should().BeTrue();
        _sut.DeletedAt.Should().BeOnOrAfter(firstDeletedAt!.Value);
    }

    private static API.Common.Results.Result<Class> CreateValid(
        int subjectId = SubjectId,
        decimal price = Price,
        DateTime? startsAt = null,
        DateTime? endsAt = null,
        DateOnly? reservedAt = null)
        => Class.Create(subjectId, price, startsAt ?? StartsAt, endsAt ?? EndsAt, reservedAt ?? ReservedAt);

    private void AssertUnchanged()
    {
        _sut.SubjectId.Should().Be(SubjectId);
        _sut.Price.Should().Be(Price);
        _sut.StartsAt.Should().Be(StartsAt);
        _sut.EndsAt.Should().Be(EndsAt);
        _sut.ReservedAt.Should().Be(ReservedAt);
    }

    private static void AssertFails(API.Common.Results.Result result, Error expected)
    {
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle().Which.Should().Be(expected);
    }
}
