using AwesomeAssertions;
using CodeWithMixx.API.Common.Result;
using CodeWithMixx.API.Domain.Entities.Subjects;
using Xunit;

namespace CodeWithMixx.UnitTests.Domain.Entities.Subjects;

public class SubjectErrorTests
{
    private const int Id = 42;

    [Fact]
    public void NameRequired_ShouldBeValidationError()
    {
        var error = SubjectError.NameRequired();

        error.Code.Should().Be("Subject.NameRequired");
        error.Description.Should().Be("Subject name is required");
        error.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public void NameTooLong_ShouldIncludeMaxLengthInMessage()
    {
        var error = SubjectError.NameTooLong();

        error.Code.Should().Be("Subject.NameTooLong");
        error.Description.Should().Be($"Subject name cannot exceed {SubjectConstants.MaxNameLength} characters");
        error.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public void DescriptionRequired_ShouldBeValidationError()
    {
        var error = SubjectError.DescriptionRequired();

        error.Code.Should().Be("Subject.DescriptionRequired");
        error.Description.Should().Be("Subject description is required");
        error.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public void DescriptionTooLong_ShouldIncludeMaxLengthInMessage()
    {
        var error = SubjectError.DescriptionTooLong();

        error.Code.Should().Be("Subject.DescriptionTooLong");
        error.Description.Should().Be($"Subject description cannot exceed {SubjectConstants.MaxDescriptionLength} characters");
        error.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public void NotFound_WithoutIdentifier_ShouldUseGenericMessage()
    {
        var error = SubjectError.NotFound();

        error.Code.Should().Be("Subject.NotFound");
        error.Description.Should().Be("Subject not found");
        error.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public void NotFound_WithIdentifier_ShouldIncludeIdentifierInMessage()
        => SubjectError.NotFound(Id).Description.Should().Be("Subject with identifier '42' is not found");

    [Fact]
    public void MultipleSubjectsMissing_ShouldListAllIdentifiers()
    {
        var error = SubjectError.MultipleSubjectsMissing([1, 2, 3]);

        error.Code.Should().Be("Subject.MultipleSubjectsMissing");
        error.Description.Should().Be("Subjects with identifiers '1, 2, 3' are not found");
        error.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public void MultipleSubjectsMissing_WithSingleIdentifier_ShouldListIt()
        => SubjectError.MultipleSubjectsMissing([7]).Description.Should().Be("Subjects with identifiers '7' are not found");

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void AlreadyExists_WithBlankIdentifier_ShouldUseGenericMessage(string? identifier)
    {
        var error = SubjectError.AlreadyExists(identifier!);

        error.Code.Should().Be("Subject.AlreadyExists");
        error.Description.Should().Be("Subject already exists");
        error.Type.Should().Be(ErrorType.Conflict);
    }

    [Fact]
    public void AlreadyExists_WithoutArgument_ShouldUseGenericMessage()
        => SubjectError.AlreadyExists().Description.Should().Be("Subject already exists");

    [Fact]
    public void AlreadyExists_WithIdentifier_ShouldIncludeIdentifierInMessage()
        => SubjectError.AlreadyExists("Matematika").Description.Should().Be("Subject with identifier 'Matematika' already exists");

    [Fact]
    public void AlreadyArchived_WithoutIdentifier_ShouldUseGenericMessage()
    {
        var error = SubjectError.AlreadyArchived();

        error.Code.Should().Be("Subject.AlreadyArchived");
        error.Description.Should().Be("Subject is already archived");
        error.Type.Should().Be(ErrorType.Conflict);
    }

    [Fact]
    public void AlreadyArchived_WithIdentifier_ShouldIncludeIdentifierInMessage()
        => SubjectError.AlreadyArchived(Id).Description.Should().Be("Subject with identifier '42' is already archived");

    [Fact]
    public void NotArchived_WithoutIdentifier_ShouldUseGenericMessage()
    {
        var error = SubjectError.NotArchived();

        error.Code.Should().Be("Subject.NotArchived");
        error.Description.Should().Be("Subject is not archived");
        error.Type.Should().Be(ErrorType.Conflict);
    }

    [Fact]
    public void NotArchived_WithIdentifier_ShouldIncludeIdentifierInMessage()
        => SubjectError.NotArchived(Id).Description.Should().Be("Subject with identifier '42' is not archived");

    [Fact]
    public void HasAssociatedClasses_WithoutIdentifier_ShouldUseGenericMessage()
    {
        var error = SubjectError.HasAssociatedClasses();

        error.Code.Should().Be("Subject.HasClasses");
        error.Description.Should().Be("Subject has classes associated with it");
        error.Type.Should().Be(ErrorType.Conflict);
    }

    [Fact]
    public void HasAssociatedClasses_WithIdentifier_ShouldIncludeIdentifierInMessage()
        => SubjectError.HasAssociatedClasses(Id).Description.Should().Be("Subject with identifier '42' has classes associated with it");
}
