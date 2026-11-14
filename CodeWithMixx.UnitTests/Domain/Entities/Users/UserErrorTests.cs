using AwesomeAssertions;
using CodeWithMixx.API.Common.Result;
using CodeWithMixx.API.Domain.Entities.Users;
using Xunit;

namespace CodeWithMixx.UnitTests.Domain.Entities.Users;

public class UserErrorTests
{
    private const string Id = "user-123";

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void EmailAlreadyExists_WithBlankEmail_ShouldUseGenericMessage(string? email)
    {
        var error = UserError.EmailAlreadyExists(email!);

        error.Code.Should().Be("User.EmailAlreadyExists");
        error.Description.Should().Be("Email is taken");
        error.Type.Should().Be(ErrorType.Conflict);
    }

    [Fact]
    public void EmailAlreadyExists_WithEmail_ShouldIncludeEmailInMessage()
    {
        var error = UserError.EmailAlreadyExists("a@b.com");

        error.Description.Should().Be("Email 'a@b.com' is taken");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void UsernameAlreadyExists_WithBlankUsername_ShouldUseGenericMessage(string? username)
    {
        var error = UserError.UsernameAlreadyExists(username!);

        error.Code.Should().Be("User.UsernameAlreadyExists");
        error.Description.Should().Be("Username is taken");
        error.Type.Should().Be(ErrorType.Conflict);
    }

    [Fact]
    public void UsernameAlreadyExists_WithUsername_ShouldIncludeUsernameInMessage()
    {
        UserError.UsernameAlreadyExists("petar").Description.Should().Be("Username 'petar' is taken");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void NotFound_WithBlankIdentifier_ShouldUseGenericMessage(string? identifier)
    {
        var error = UserError.NotFound(identifier!);

        error.Code.Should().Be("User.NotFound");
        error.Description.Should().Be("User not found");
        error.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public void NotFound_WithIdentifier_ShouldIncludeIdentifierInMessage()
    {
        UserError.NotFound(Id).Description.Should().Be($"User with identifier '{Id}' is not found");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void CannotUpdatePhoneNumber_WithoutIdentityErrorCode_ShouldUseShortMessage(string? identityErrorCode)
    {
        var error = UserError.CannotUpdatePhoneNumber(Id, identityErrorCode);

        error.Code.Should().Be("User.CannotUpdatePhoneNumber");
        error.Description.Should().Be($"User with identifier '{Id}' cannot update his phone number");
        error.Type.Should().Be(ErrorType.Conflict);
    }

    [Fact]
    public void CannotUpdatePhoneNumber_WithIdentityErrorCode_ShouldIncludeCodeInMessage()
    {
        var error = UserError.CannotUpdatePhoneNumber(Id, "InvalidPhoneNumber");

        error.Description.Should().Contain("'InvalidPhoneNumber'");
    }

    public static TheoryData<Func<string, Error>, string> IdentifierErrors => new()
    {
        { UserError.NotAStudent, "User.NotAStudent" },
        { UserError.CannotChangeStatusForDeletedUser, "User.CannotChangeStatusForDeletedUser" },
        { UserError.CannotActivateWithNullPassword, "User.CannotActivateWithNullPassword" },
        { UserError.AlreadyActivated, "User.AlreadyActivated" },
        { UserError.AlreadyDeactivated, "User.AlreadyDeactivated" },
        { UserError.AlreadyDeleted, "User.AlreadyDeleted" },
    };

    public static TheoryData<Func<Error>, string> NameErrors => new()
    {
        { UserError.FirstNameRequired, "User.FirstNameRequired" },
        { UserError.FirstNameTooLong, "User.FirstNameTooLong" },
        { UserError.LastNameRequired, "User.LastNameRequired" },
        { UserError.LastNameTooLong, "User.LastNameTooLong" },
    };

    [Theory]
    [MemberData(nameof(NameErrors))]
    public void NameErrors_ShouldBeValidationErrors(Func<Error> factory, string expectedCode)
    {
        var error = factory();

        error.Code.Should().Be(expectedCode);
        error.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public void TooLongErrors_ShouldMentionMaxLength()
    {
        UserError.FirstNameTooLong().Description.Should().Contain(UserConstants.MaxNameLength.ToString());
        UserError.LastNameTooLong().Description.Should().Contain(UserConstants.MaxNameLength.ToString());
    }

    [Theory]
    [MemberData(nameof(IdentifierErrors))]
    public void IdentifierErrors_ShouldBeConflictWithCodeAndIdentifierInMessage(Func<string, Error> factory, string expectedCode)
    {
        var error = factory(Id);

        error.Code.Should().Be(expectedCode);
        error.Type.Should().Be(ErrorType.Conflict);
        error.Description.Should().Contain($"'{Id}'");
    }
}
