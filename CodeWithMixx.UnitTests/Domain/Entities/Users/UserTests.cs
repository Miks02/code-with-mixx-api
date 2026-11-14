using System.Reflection;
using AwesomeAssertions;
using CodeWithMixx.API.Domain.Entities.Students;
using CodeWithMixx.API.Domain.Entities.Users;
using Xunit;

namespace CodeWithMixx.UnitTests.Domain.Entities.Users;

public class UserTests
{
    private const string FirstName = "Petar";
    private const string LastName = "Petrovic";
    private const string Email = "petar@example.com";
    private const string PhoneNumber = "+381601234567";

    private readonly User _sut;

    public UserTests()
    {
        _sut = User.CreateUser(FirstName, LastName, Email, PhoneNumber);
    }

    [Fact]
    public void CreateUser_ShouldMapProvidedValues()
    {
        _sut.FirstName.Should().Be(FirstName);
        _sut.LastName.Should().Be(LastName);
        _sut.Email.Should().Be(Email);
        _sut.PhoneNumber.Should().Be(PhoneNumber);
    }

    [Fact]
    public void CreateUser_ShouldUseEmailAsUserName()
    {
        _sut.UserName.Should().Be(Email);
    }

    [Fact]
    public void CreateUser_ShouldStartAsPendingWithUnconfirmedEmailAndNoPassword()
    {
        _sut.AccountStatus.Should().Be(AccountStatus.Pending);
        _sut.EmailConfirmed.Should().BeFalse();
        _sut.PhoneNumberConfirmed.Should().BeFalse();
        _sut.PasswordHash.Should().BeNull();
    }

    [Fact]
    public void CreateUser_ShouldStartNotDeletedWithoutLoginOrRelations()
    {
        _sut.IsDeleted.Should().BeFalse();
        _sut.DeletedAt.Should().BeNull();
        _sut.LastLoginAt.Should().BeNull();
        _sut.Student.Should().BeNull();
        _sut.Admin.Should().BeNull();
        _sut.RefreshTokens.Should().BeEmpty();
    }

    [Fact]
    public void CreateUser_ShouldGenerateUniqueIds()
    {
        var other = User.CreateUser(FirstName, LastName, Email, PhoneNumber);

        _sut.Id.Should().NotBeNullOrWhiteSpace();
        other.Id.Should().NotBe(_sut.Id);
    }

    [Fact]
    public void CreateAdmin_ShouldStartActiveWithConfirmedEmailAndPhone()
    {
        var admin = User.CreateAdmin(FirstName, LastName, Email, PhoneNumber);

        admin.AccountStatus.Should().Be(AccountStatus.Active);
        admin.EmailConfirmed.Should().BeTrue();
        admin.PhoneNumberConfirmed.Should().BeTrue();
        admin.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public void CreateAdmin_ShouldMapProvidedValuesAndUseEmailAsUserName()
    {
        var admin = User.CreateAdmin(FirstName, LastName, Email, PhoneNumber);

        admin.FirstName.Should().Be(FirstName);
        admin.LastName.Should().Be(LastName);
        admin.Email.Should().Be(Email);
        admin.UserName.Should().Be(Email);
        admin.PhoneNumber.Should().Be(PhoneNumber);
    }

    [Fact]
    public void ChangeName_WithValidValues_ShouldUpdateFirstAndLastName()
    {
        var result = _sut.ChangeName("Marko", "Markovic");

        result.IsSuccess.Should().BeTrue();
        _sut.FirstName.Should().Be("Marko");
        _sut.LastName.Should().Be("Markovic");
    }

    [Fact]
    public void ChangeName_ShouldNotTouchOtherFields()
    {
        _sut.ChangeName("Marko", "Markovic");

        _sut.Email.Should().Be(Email);
        _sut.UserName.Should().Be(Email);
        _sut.AccountStatus.Should().Be(AccountStatus.Pending);
    }

    [Fact]
    public void ChangeName_WithSurroundingWhitespace_ShouldStoreTrimmedValues()
    {
        var result = _sut.ChangeName("  Marko ", "\tMarkovic\n");

        result.IsSuccess.Should().BeTrue();
        _sut.FirstName.Should().Be("Marko");
        _sut.LastName.Should().Be("Markovic");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t\n")]
    public void ChangeName_WithBlankFirstName_ShouldFailAndKeepOriginalName(string firstName)
    {
        var result = _sut.ChangeName(firstName, "Markovic");

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle().Which.Should().Be(UserError.FirstNameRequired());
        _sut.FirstName.Should().Be(FirstName);
        _sut.LastName.Should().Be(LastName);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t\n")]
    public void ChangeName_WithBlankLastName_ShouldFailAndKeepOriginalName(string lastName)
    {
        var result = _sut.ChangeName("Marko", lastName);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle().Which.Should().Be(UserError.LastNameRequired());
        // First name must not be partially applied when the last name is invalid.
        _sut.FirstName.Should().Be(FirstName);
        _sut.LastName.Should().Be(LastName);
    }

    [Fact]
    public void ChangeName_WhenBothBlank_ShouldReportFirstNameError()
    {
        var result = _sut.ChangeName("", "");

        result.Errors.Should().ContainSingle().Which.Code.Should().Be("User.FirstNameRequired");
    }

    [Fact]
    public void ChangeName_WithNamesAtMaxLength_ShouldSucceed()
    {
        var name = new string('a', UserConstants.MaxNameLength);

        var result = _sut.ChangeName(name, name);

        result.IsSuccess.Should().BeTrue();
        _sut.FirstName.Should().Be(name);
        _sut.LastName.Should().Be(name);
    }

    [Fact]
    public void ChangeName_WithFirstNameOverMaxLength_ShouldFail()
    {
        var result = _sut.ChangeName(new string('a', UserConstants.MaxNameLength + 1), "Markovic");

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle().Which.Should().Be(UserError.FirstNameTooLong());
        _sut.FirstName.Should().Be(FirstName);
    }

    [Fact]
    public void ChangeName_WithLastNameOverMaxLength_ShouldFail()
    {
        var result = _sut.ChangeName("Marko", new string('a', UserConstants.MaxNameLength + 1));

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle().Which.Should().Be(UserError.LastNameTooLong());
        _sut.LastName.Should().Be(LastName);
    }

    // Length is checked after trimming, so padding doesn't push a valid name over the limit.
    [Fact]
    public void ChangeName_WithPaddedNameThatFitsAfterTrim_ShouldSucceed()
    {
        var name = new string('a', UserConstants.MaxNameLength);

        var result = _sut.ChangeName($"  {name}  ", "Markovic");

        result.IsSuccess.Should().BeTrue();
        _sut.FirstName.Should().Be(name);
    }

    [Fact]
    public void UpdateLastLogin_ShouldSetLastLoginToCurrentUtcTime()
    {
        var before = DateTime.UtcNow;

        _sut.UpdateLastLogin();

        var after = DateTime.UtcNow;
        _sut.LastLoginAt.Should().NotBeNull();
        _sut.LastLoginAt!.Value.Kind.Should().Be(DateTimeKind.Utc);
        _sut.LastLoginAt.Value.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
    }

    [Fact]
    public void UpdateLastLogin_CalledTwice_ShouldMoveLastLoginForward()
    {
        _sut.UpdateLastLogin();
        var first = _sut.LastLoginAt;

        _sut.UpdateLastLogin();

        _sut.LastLoginAt.Should().BeOnOrAfter(first!.Value);
    }

    [Fact]
    public void ConfirmEmail_WhenNotConfirmed_ShouldConfirm()
    {
        _sut.ConfirmEmail();

        _sut.EmailConfirmed.Should().BeTrue();
    }

    [Fact]
    public void ConfirmEmail_WhenAlreadyConfirmed_ShouldStayConfirmed()
    {
        _sut.ConfirmEmail();

        _sut.ConfirmEmail();

        _sut.EmailConfirmed.Should().BeTrue();
    }

    [Fact]
    public void ActivateAccount_WhenPendingWithPassword_ShouldActivate()
    {
        _sut.PasswordHash = "hash";

        var result = _sut.ActivateAccount();

        result.IsSuccess.Should().BeTrue();
        _sut.AccountStatus.Should().Be(AccountStatus.Active);
    }

    [Fact]
    public void ActivateAccount_WhenDeactivatedWithPassword_ShouldActivate()
    {
        _sut.PasswordHash = "hash";
        _sut.DeactivateAccount();

        var result = _sut.ActivateAccount();

        result.IsSuccess.Should().BeTrue();
        _sut.AccountStatus.Should().Be(AccountStatus.Active);
    }

    // Edge case: a deactivated user that never set a password is "re-activated" back into Pending,
    // so they can still finish onboarding via the invitation flow.
    [Fact]
    public void ActivateAccount_WhenDeactivatedWithoutPassword_ShouldMoveBackToPending()
    {
        _sut.DeactivateAccount();

        var result = _sut.ActivateAccount();

        result.IsSuccess.Should().BeTrue();
        _sut.AccountStatus.Should().Be(AccountStatus.Pending);
    }

    [Fact]
    public void ActivateAccount_WhenPendingWithoutPassword_ShouldReturnCannotActivateWithNullPassword()
    {
        var result = _sut.ActivateAccount();

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle()
            .Which.Should().Be(UserError.CannotActivateWithNullPassword(_sut.Id));
        _sut.AccountStatus.Should().Be(AccountStatus.Pending);
    }

    [Fact]
    public void ActivateAccount_WhenAlreadyActive_ShouldReturnAlreadyActivated()
    {
        var admin = User.CreateAdmin(FirstName, LastName, Email, PhoneNumber);

        var result = admin.ActivateAccount();

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle()
            .Which.Should().Be(UserError.AlreadyActivated(admin.Id));
        admin.AccountStatus.Should().Be(AccountStatus.Active);
    }

    [Fact]
    public void ActivateAccount_WhenDeleted_ShouldReturnCannotChangeStatusForDeletedUser()
    {
        _sut.DeleteUser();

        var result = _sut.ActivateAccount();

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle()
            .Which.Should().Be(UserError.CannotChangeStatusForDeletedUser(_sut.Id));
        _sut.AccountStatus.Should().Be(AccountStatus.Deleted);
    }

    [Fact]
    public void ActivateAccount_WhenDeletedAndPasswordSetAfterwards_ShouldStillFail()
    {
        _sut.DeleteUser();
        _sut.PasswordHash = "hash";

        var result = _sut.ActivateAccount();

        result.IsSuccess.Should().BeFalse();
        result.Errors[0].Code.Should().Be("User.CannotChangeStatusForDeletedUser");
        _sut.AccountStatus.Should().Be(AccountStatus.Deleted);
    }

    [Fact]
    public void DeactivateAccount_WhenActive_ShouldDeactivate()
    {
        var admin = User.CreateAdmin(FirstName, LastName, Email, PhoneNumber);

        var result = admin.DeactivateAccount();

        result.IsSuccess.Should().BeTrue();
        admin.AccountStatus.Should().Be(AccountStatus.Deactivated);
    }

    [Fact]
    public void DeactivateAccount_WhenPending_ShouldDeactivate()
    {
        var result = _sut.DeactivateAccount();

        result.IsSuccess.Should().BeTrue();
        _sut.AccountStatus.Should().Be(AccountStatus.Deactivated);
    }

    [Fact]
    public void DeactivateAccount_WhenAlreadyDeactivated_ShouldReturnAlreadyDeactivated()
    {
        _sut.DeactivateAccount();

        var result = _sut.DeactivateAccount();

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle()
            .Which.Should().Be(UserError.AlreadyDeactivated(_sut.Id));
        _sut.AccountStatus.Should().Be(AccountStatus.Deactivated);
    }

    [Fact]
    public void DeactivateAccount_WhenDeleted_ShouldReturnCannotChangeStatusForDeletedUser()
    {
        _sut.DeleteUser();

        var result = _sut.DeactivateAccount();

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle()
            .Which.Should().Be(UserError.CannotChangeStatusForDeletedUser(_sut.Id));
        _sut.AccountStatus.Should().Be(AccountStatus.Deleted);
    }

    [Fact]
    public void DeactivateThenActivate_RoundTrip_ShouldEndActive()
    {
        var admin = User.CreateAdmin(FirstName, LastName, Email, PhoneNumber);
        admin.PasswordHash = "hash";

        admin.DeactivateAccount().IsSuccess.Should().BeTrue();
        admin.ActivateAccount().IsSuccess.Should().BeTrue();

        admin.AccountStatus.Should().Be(AccountStatus.Active);
    }

    [Fact]
    public void DeleteUser_ShouldMarkAsDeletedWithUtcTimestamp()
    {
        var before = DateTime.UtcNow;

        var result = _sut.DeleteUser();

        var after = DateTime.UtcNow;
        result.IsSuccess.Should().BeTrue();
        _sut.IsDeleted.Should().BeTrue();
        _sut.AccountStatus.Should().Be(AccountStatus.Deleted);
        _sut.DeletedAt.Should().NotBeNull();
        _sut.DeletedAt!.Value.Kind.Should().Be(DateTimeKind.Utc);
        _sut.DeletedAt.Value.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
    }

    [Fact]
    public void DeleteUser_ShouldAnonymizePersonalData()
    {
        _sut.PasswordHash = "hash";

        _sut.DeleteUser();

        var anonymizedEmail = $"deleted_{_sut.Id}@deleted.invalid";
        _sut.Email.Should().Be(anonymizedEmail);
        _sut.UserName.Should().Be(anonymizedEmail);
        _sut.FirstName.Should().Be("Deleted");
        _sut.LastName.Should().Be("Deleted");
        _sut.PhoneNumber.Should().BeNull();
        _sut.PasswordHash.Should().BeNull();
    }

    [Fact]
    public void DeleteUser_ShouldNotLeakOriginalEmailIntoAnonymizedValues()
    {
        _sut.DeleteUser();

        _sut.Email.Should().NotContain(Email);
        _sut.UserName.Should().NotContain(Email);
    }

    [Fact]
    public void DeleteUser_TwoDifferentUsers_ShouldGetUniqueAnonymizedEmails()
    {
        var other = User.CreateUser(FirstName, LastName, Email, PhoneNumber);

        _sut.DeleteUser();
        other.DeleteUser();

        other.Email.Should().NotBe(_sut.Email);
        other.UserName.Should().NotBe(_sut.UserName);
    }

    [Fact]
    public void DeleteUser_WhenActive_ShouldOverrideStatusToDeleted()
    {
        var admin = User.CreateAdmin(FirstName, LastName, Email, PhoneNumber);

        admin.DeleteUser();

        admin.AccountStatus.Should().Be(AccountStatus.Deleted);
    }

    [Fact]
    public void DeleteUser_WhenDeactivated_ShouldOverrideStatusToDeleted()
    {
        _sut.DeactivateAccount();

        _sut.DeleteUser();

        _sut.AccountStatus.Should().Be(AccountStatus.Deleted);
    }

    [Fact]
    public void DeleteUser_WithoutStudentProfile_ShouldNotThrow()
    {
        var act = () => _sut.DeleteUser();

        act.Should().NotThrow();
    }

    [Fact]
    public void DeleteUser_WithStudentProfile_ShouldAlsoSoftDeleteStudent()
    {
        var student = Student.Create(_sut.Id, "ETF Beograd");
        AttachStudent(_sut, student);

        _sut.DeleteUser();

        student.IsDeleted.Should().BeTrue();
        student.DeletedAt.Should().NotBeNull();
        student.University.Should().BeNull();
    }

    [Fact]
    public void DeleteUser_WhenAlreadyDeleted_ShouldReturnAlreadyDeletedAndKeepOriginalState()
    {
        _sut.DeleteUser();
        var firstDeletedAt = _sut.DeletedAt;
        var anonymizedEmail = _sut.Email;

        var result = _sut.DeleteUser();

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle()
            .Which.Should().Be(UserError.AlreadyDeleted(_sut.Id));
        _sut.DeletedAt.Should().Be(firstDeletedAt);
        _sut.Email.Should().Be(anonymizedEmail);
        _sut.AccountStatus.Should().Be(AccountStatus.Deleted);
    }

    [Fact]
    public void DeleteUser_WhenAlreadyDeleted_ShouldNotTouchStudentAgain()
    {
        var student = Student.Create(_sut.Id, "ETF Beograd");
        AttachStudent(_sut, student);
        _sut.DeleteUser();
        var studentDeletedAt = student.DeletedAt;

        _sut.DeleteUser();

        student.DeletedAt.Should().Be(studentDeletedAt);
    }

    // Student is a get-only navigation set by EF Core; tests populate it the same way (via backing field).
    private static void AttachStudent(User user, Student student)
    {
        var field = typeof(User).GetField("<Student>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic)
                    ?? throw new InvalidOperationException("Student backing field not found on User.");
        field.SetValue(user, student);
    }
}
