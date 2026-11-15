using AwesomeAssertions;
using CodeWithMixx.API.Domain.Entities.Reservations;
using CodeWithMixx.API.Domain.Entities.Students;
using Xunit;

namespace CodeWithMixx.UnitTests.Domain.Entities.Students;

public class StudentTests
{
    private const string UserId = "user-123";
    private const string University = "ETF Beograd";

    private readonly Student _sut;

    public StudentTests()
    {
        _sut = Student.Create(UserId, University).Payload!;
    }

    [Fact]
    public void Create_WithValidValues_ShouldSucceedAndMapValues()
    {
        var result = Student.Create(UserId, University);

        result.IsSuccess.Should().BeTrue();
        result.Payload!.UserId.Should().Be(UserId);
        result.Payload.University.Should().Be(University);
    }

    [Fact]
    public void Create_ShouldStartNotDeletedWithoutReservations()
    {
        _sut.IsDeleted.Should().BeFalse();
        _sut.DeletedAt.Should().BeNull();
        _sut.Reservations.Should().BeEmpty();
    }

    [Fact]
    public void Create_WithoutUniversity_ShouldLeaveUniversityNull()
    {
        var result = Student.Create(UserId, null);

        result.IsSuccess.Should().BeTrue();
        result.Payload!.University.Should().BeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t\n")]
    public void Create_WithBlankUniversity_ShouldNormalizeToNull(string university)
    {
        var result = Student.Create(UserId, university);

        result.IsSuccess.Should().BeTrue();
        result.Payload!.University.Should().BeNull();
    }

    [Fact]
    public void Create_WithSurroundingWhitespace_ShouldTrimUniversity()
    {
        var result = Student.Create(UserId, "  FON  ");

        result.Payload!.University.Should().Be("FON");
    }

    [Fact]
    public void Create_WithUniversityAtMaxLength_ShouldSucceed()
    {
        var name = new string('a', StudentConstants.MaxUniversityLength);

        var result = Student.Create(UserId, name);

        result.IsSuccess.Should().BeTrue();
        result.Payload!.University.Should().Be(name);
    }

    [Fact]
    public void Create_WithUniversityOverMaxLength_ShouldFail()
    {
        var result = Student.Create(UserId, new string('a', StudentConstants.MaxUniversityLength + 1));

        result.IsSuccess.Should().BeFalse();
        result.Payload.Should().BeNull();
        result.Errors.Should().ContainSingle().Which.Should().Be(StudentError.UniversityTooLong());
    }

    // Length is checked after trimming, so padding doesn't push a valid name over the limit.
    [Fact]
    public void Create_WithPaddedUniversityThatFitsAfterTrim_ShouldSucceed()
    {
        var name = new string('a', StudentConstants.MaxUniversityLength);

        var result = Student.Create(UserId, $"  {name}  ");

        result.IsSuccess.Should().BeTrue();
        result.Payload!.University.Should().Be(name);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithBlankUserId_ShouldFail(string? userId)
    {
        var result = Student.Create(userId!, University);

        result.IsSuccess.Should().BeFalse();
        result.Payload.Should().BeNull();
        result.Errors.Should().ContainSingle().Which.Should().Be(StudentError.UserIdRequired());
    }

    [Fact]
    public void Create_WithBlankUserIdAndTooLongUniversity_ShouldReportUserIdFirst()
    {
        var result = Student.Create("", new string('a', StudentConstants.MaxUniversityLength + 1));

        result.Errors.Should().ContainSingle().Which.Code.Should().Be("Student.UserIdRequired");
    }

    [Fact]
    public void Reservations_ShouldBeReadOnly()
    {
        _sut.Reservations.Should().NotBeAssignableTo<List<Reservation>>();
    }

    [Fact]
    public void ChangeUniversity_WithValidValue_ShouldUpdateUniversity()
    {
        var result = _sut.ChangeUniversity("FON");

        result.IsSuccess.Should().BeTrue();
        _sut.University.Should().Be("FON");
    }

    [Fact]
    public void ChangeUniversity_WithNull_ShouldClearUniversity()
    {
        var result = _sut.ChangeUniversity(null);

        result.IsSuccess.Should().BeTrue();
        _sut.University.Should().BeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t\n")]
    public void ChangeUniversity_WithBlankValue_ShouldClearUniversity(string university)
    {
        var result = _sut.ChangeUniversity(university);

        result.IsSuccess.Should().BeTrue();
        _sut.University.Should().BeNull();
    }

    [Fact]
    public void ChangeUniversity_WithSurroundingWhitespace_ShouldStoreTrimmedValue()
    {
        _sut.ChangeUniversity("  FON  ");

        _sut.University.Should().Be("FON");
    }

    [Fact]
    public void ChangeUniversity_WithSameValue_ShouldSucceed()
    {
        var result = _sut.ChangeUniversity(University);

        result.IsSuccess.Should().BeTrue();
        _sut.University.Should().Be(University);
    }

    [Fact]
    public void ChangeUniversity_WithValueAtMaxLength_ShouldSucceed()
    {
        var name = new string('a', StudentConstants.MaxUniversityLength);

        var result = _sut.ChangeUniversity(name);

        result.IsSuccess.Should().BeTrue();
        _sut.University.Should().Be(name);
    }

    [Fact]
    public void ChangeUniversity_WithValueOverMaxLength_ShouldFailAndKeepOriginal()
    {
        var result = _sut.ChangeUniversity(new string('a', StudentConstants.MaxUniversityLength + 1));

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle().Which.Should().Be(StudentError.UniversityTooLong());
        _sut.University.Should().Be(University);
    }

    [Fact]
    public void ChangeUniversity_ShouldNotTouchOtherFields()
    {
        _sut.ChangeUniversity("FON");

        _sut.UserId.Should().Be(UserId);
        _sut.IsDeleted.Should().BeFalse();
        _sut.DeletedAt.Should().BeNull();
    }

    [Fact]
    public void ChangeUniversity_WhenDeleted_ShouldFailAndKeepUniversityCleared()
    {
        _sut.Delete();

        var result = _sut.ChangeUniversity("FON");

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle().Which.Should().Be(StudentError.CannotModifyDeleted(UserId));
        _sut.University.Should().BeNull();
    }

    [Fact]
    public void ChangeUniversity_WhenDeletedAndClearing_ShouldStillFail()
    {
        _sut.Delete();

        var result = _sut.ChangeUniversity(null);

        result.IsSuccess.Should().BeFalse();
        result.Errors[0].Code.Should().Be("Student.CannotModifyDeleted");
    }

    [Fact]
    public void Delete_ShouldMarkAsDeletedWithUtcTimestamp()
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
    public void Delete_ShouldClearUniversity()
    {
        _sut.Delete();

        _sut.University.Should().BeNull();
    }

    [Fact]
    public void Delete_ShouldKeepUserId()
    {
        _sut.Delete();

        _sut.UserId.Should().Be(UserId);
    }

    [Fact]
    public void Delete_WithoutUniversity_ShouldSucceed()
    {
        var student = Student.Create(UserId, null).Payload!;

        var result = student.Delete();

        result.IsSuccess.Should().BeTrue();
        student.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public void Delete_WhenAlreadyDeleted_ShouldFailAndKeepOriginalDeletedAt()
    {
        _sut.Delete();
        var firstDeletedAt = _sut.DeletedAt;

        var result = _sut.Delete();

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle().Which.Should().Be(StudentError.AlreadyDeleted(UserId));
        _sut.DeletedAt.Should().Be(firstDeletedAt);
        _sut.IsDeleted.Should().BeTrue();
    }
}
