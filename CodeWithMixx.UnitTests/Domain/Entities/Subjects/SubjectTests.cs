using AwesomeAssertions;
using CodeWithMixx.API.Common.Result;
using CodeWithMixx.API.Domain.Entities.Classes;
using CodeWithMixx.API.Domain.Entities.Projects;
using CodeWithMixx.API.Domain.Entities.Subjects;
using Xunit;

namespace CodeWithMixx.UnitTests.Domain.Entities.Subjects;

public class SubjectTests
{
    private const string Name = "Programiranje 1";
    private const string Description = "Osnove programiranja u C-u.";
    private const string NewName = "Programiranje 2";
    private const string NewDescription = "Objektno orijentisano programiranje.";

    private readonly Subject _sut;

    public SubjectTests()
    {
        _sut = Subject.Create(Name, Description).Payload!;
    }

    [Fact]
    public void Create_WithValidValues_ShouldSucceedAndMapValues()
    {
        var result = Subject.Create(Name, Description);

        result.IsSuccess.Should().BeTrue();
        result.Payload!.Name.Should().Be(Name);
        result.Payload.Description.Should().Be(Description);
    }

    [Fact]
    public void Create_ShouldStartNotDeletedWithoutClassesOrProjects()
    {
        _sut.IsDeleted.Should().BeFalse();
        _sut.DeletedAt.Should().BeNull();
        _sut.Classes.Should().BeEmpty();
        _sut.Projects.Should().BeEmpty();
    }

    [Fact]
    public void Create_ShouldLeaveIdAndAuditFieldsUnset()
    {
        _sut.Id.Should().Be(0);
        _sut.CreatedAt.Should().Be(default);
        _sut.UpdatedAt.Should().BeNull();
    }

    [Fact]
    public void Create_WithSurroundingWhitespace_ShouldTrimValues()
    {
        var result = Subject.Create($"  {Name}\t", $"\n {Description}  ");

        result.Payload!.Name.Should().Be(Name);
        result.Payload.Description.Should().Be(Description);
    }

    [Theory]
    [InlineData("Đorđevićeva matematika")]
    [InlineData("C#/.NET")]
    public void Create_WithNonAsciiOrSymbolName_ShouldSucceed(string name)
        => Subject.Create(name, Description).IsSuccess.Should().BeTrue();

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t\n")]
    public void Create_WithBlankName_ShouldReturnNameRequired(string? name)
        => AssertFails(Subject.Create(name!, Description), SubjectError.NameRequired());

    [Fact]
    public void Create_WithNameAtMaxLength_ShouldSucceed()
        => Subject.Create(new string('a', SubjectConstants.MaxNameLength), Description).IsSuccess.Should().BeTrue();

    [Fact]
    public void Create_WithNameOverMaxLength_ShouldReturnNameTooLong()
        => AssertFails(Subject.Create(new string('a', SubjectConstants.MaxNameLength + 1), Description), SubjectError.NameTooLong());

    [Fact]
    public void Create_WithPaddedNameThatFitsAfterTrim_ShouldSucceed()
        => Subject.Create($"  {new string('a', SubjectConstants.MaxNameLength)}  ", Description).IsSuccess.Should().BeTrue();

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t\n")]
    public void Create_WithBlankDescription_ShouldReturnDescriptionRequired(string? description)
        => AssertFails(Subject.Create(Name, description!), SubjectError.DescriptionRequired());

    [Fact]
    public void Create_WithDescriptionAtMaxLength_ShouldSucceed()
        => Subject.Create(Name, new string('a', SubjectConstants.MaxDescriptionLength)).IsSuccess.Should().BeTrue();

    [Fact]
    public void Create_WithDescriptionOverMaxLength_ShouldReturnDescriptionTooLong()
        => AssertFails(Subject.Create(Name, new string('a', SubjectConstants.MaxDescriptionLength + 1)), SubjectError.DescriptionTooLong());

    [Fact]
    public void Create_WithPaddedDescriptionThatFitsAfterTrim_ShouldSucceed()
        => Subject.Create(Name, $"  {new string('a', SubjectConstants.MaxDescriptionLength)}  ").IsSuccess.Should().BeTrue();

    [Fact]
    public void Create_WithBlankNameAndDescription_ShouldReportNameFirst()
        => AssertFails(Subject.Create("", ""), SubjectError.NameRequired());

    [Fact]
    public void Create_WithTooLongNameAndBlankDescription_ShouldReportNameFirst()
        => AssertFails(Subject.Create(new string('a', SubjectConstants.MaxNameLength + 1), ""), SubjectError.NameTooLong());

    [Fact]
    public void Classes_ShouldNotBeMutableList()
        => _sut.Classes.Should().NotBeAssignableTo<List<Class>>();

    [Fact]
    public void Projects_ShouldNotBeMutableList()
        => _sut.Projects.Should().NotBeAssignableTo<List<Project>>();

    [Fact]
    public void Update_WithValidValues_ShouldSucceedAndReplaceValues()
    {
        var result = _sut.Update(NewName, NewDescription);

        result.IsSuccess.Should().BeTrue();
        _sut.Name.Should().Be(NewName);
        _sut.Description.Should().Be(NewDescription);
    }

    [Fact]
    public void Update_WithSurroundingWhitespace_ShouldTrimValues()
    {
        _sut.Update($"  {NewName} ", $" {NewDescription}\n");

        _sut.Name.Should().Be(NewName);
        _sut.Description.Should().Be(NewDescription);
    }

    [Fact]
    public void Update_WithSameValues_ShouldSucceed()
        => _sut.Update(Name, Description).IsSuccess.Should().BeTrue();

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Update_WithBlankName_ShouldReturnNameRequiredAndKeepValues(string? name)
    {
        var result = _sut.Update(name!, NewDescription);

        AssertFails(result, SubjectError.NameRequired());
        AssertUnchanged();
    }

    [Fact]
    public void Update_WithNameOverMaxLength_ShouldReturnNameTooLongAndKeepValues()
    {
        var result = _sut.Update(new string('a', SubjectConstants.MaxNameLength + 1), NewDescription);

        AssertFails(result, SubjectError.NameTooLong());
        AssertUnchanged();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Update_WithBlankDescription_ShouldReturnDescriptionRequiredAndKeepValues(string? description)
    {
        var result = _sut.Update(NewName, description!);

        AssertFails(result, SubjectError.DescriptionRequired());
        AssertUnchanged();
    }

    [Fact]
    public void Update_WithDescriptionOverMaxLength_ShouldReturnDescriptionTooLongAndKeepValues()
    {
        var result = _sut.Update(NewName, new string('a', SubjectConstants.MaxDescriptionLength + 1));

        AssertFails(result, SubjectError.DescriptionTooLong());
        AssertUnchanged();
    }

    [Fact]
    public void Update_ShouldNotChangeDeletionState()
    {
        _sut.Archive();
        var deletedAt = _sut.DeletedAt;

        _sut.Update(NewName, NewDescription);

        _sut.IsDeleted.Should().BeTrue();
        _sut.DeletedAt.Should().Be(deletedAt);
    }

    [Fact]
    public void Archive_WhenNotArchived_ShouldSucceedWithUtcTimestamp()
    {
        var before = DateTime.UtcNow;

        var result = _sut.Archive();

        var after = DateTime.UtcNow;
        result.IsSuccess.Should().BeTrue();
        _sut.IsDeleted.Should().BeTrue();
        _sut.DeletedAt.Should().NotBeNull();
        _sut.DeletedAt!.Value.Kind.Should().Be(DateTimeKind.Utc);
        _sut.DeletedAt.Value.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
    }

    [Fact]
    public void Archive_ShouldNotChangeNameOrDescription()
    {
        _sut.Archive();

        AssertUnchanged();
    }

    [Fact]
    public void Archive_WhenAlreadyArchived_ShouldReturnAlreadyArchivedAndKeepOriginalDeletedAt()
    {
        _sut.Archive();
        var firstDeletedAt = _sut.DeletedAt;

        var result = _sut.Archive();

        AssertFails(result, SubjectError.AlreadyArchived(_sut.Id));
        _sut.IsDeleted.Should().BeTrue();
        _sut.DeletedAt.Should().Be(firstDeletedAt);
    }

    [Fact]
    public void Delete_WhenNotArchived_ShouldArchive()
    {
        var result = _sut.Delete();

        result.IsSuccess.Should().BeTrue();
        _sut.IsDeleted.Should().BeTrue();
        _sut.DeletedAt.Should().NotBeNull();
    }

    [Fact]
    public void Delete_WhenAlreadyArchived_ShouldReturnAlreadyArchivedAndKeepOriginalDeletedAt()
    {
        _sut.Archive();
        var firstDeletedAt = _sut.DeletedAt;

        var result = _sut.Delete();

        AssertFails(result, SubjectError.AlreadyArchived(_sut.Id));
        _sut.DeletedAt.Should().Be(firstDeletedAt);
    }

    [Fact]
    public void Restore_WhenArchived_ShouldSucceedAndClearDeletion()
    {
        _sut.Archive();

        var result = _sut.Restore();

        result.IsSuccess.Should().BeTrue();
        _sut.IsDeleted.Should().BeFalse();
        _sut.DeletedAt.Should().BeNull();
    }

    [Fact]
    public void Restore_WhenNotArchived_ShouldReturnNotArchived()
    {
        var result = _sut.Restore();

        AssertFails(result, SubjectError.NotArchived(_sut.Id));
        _sut.IsDeleted.Should().BeFalse();
        _sut.DeletedAt.Should().BeNull();
    }

    [Fact]
    public void Restore_WhenRestoredTwice_ShouldFailSecondTime()
    {
        _sut.Archive();
        _sut.Restore();

        AssertFails(_sut.Restore(), SubjectError.NotArchived(_sut.Id));
    }

    [Fact]
    public void Archive_AfterRestore_ShouldSucceedWithNewDeletedAt()
    {
        _sut.Archive();
        var firstDeletedAt = _sut.DeletedAt;
        _sut.Restore();

        var result = _sut.Archive();

        result.IsSuccess.Should().BeTrue();
        _sut.IsDeleted.Should().BeTrue();
        _sut.DeletedAt.Should().BeOnOrAfter(firstDeletedAt!.Value);
    }

    private void AssertUnchanged()
    {
        _sut.Name.Should().Be(Name);
        _sut.Description.Should().Be(Description);
    }

    private static void AssertFails(API.Common.Results.Result result, Error expected)
    {
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle().Which.Should().Be(expected);
    }
}
