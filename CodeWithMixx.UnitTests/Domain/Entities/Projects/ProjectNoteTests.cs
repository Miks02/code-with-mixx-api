using AwesomeAssertions;
using CodeWithMixx.API.Domain.Entities.Projects;
using Xunit;

namespace CodeWithMixx.UnitTests.Domain.Entities.Projects;

public class ProjectNoteTests
{
    private const string Content = "Klijent želi ASP.NET backend.";

    [Fact]
    public void Create_WithContent_ShouldSucceedAndMapContent()
    {
        var result = ProjectNote.Create(Content);

        result.IsSuccess.Should().BeTrue();
        result.Payload!.Content.Should().Be(Content);
    }

    [Fact]
    public void Create_ShouldLeaveIdsAndAuditFieldsUnset()
    {
        var note = ProjectNote.Create(Content).Payload!;

        note.Id.Should().Be(0);
        note.ProjectId.Should().Be(0);
        note.CreatedAt.Should().Be(default);
        note.UpdatedAt.Should().BeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t\n")]
    public void Create_WithBlankContent_ShouldReturnEmptyProjectNote(string? content)
    {
        var result = ProjectNote.Create(content!);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle().Which.Should().Be(ProjectError.EmptyProjectNote());
    }

    [Fact]
    public void Create_WithPaddedContent_ShouldCurrentlyStoreItUntrimmed()
        => ProjectNote.Create($"  {Content} ").Payload!.Content.Should().Be($"  {Content} ");

    [Fact]
    public void Create_WithContentAtMaxLength_ShouldSucceed()
        => ProjectNote.Create(new string('a', ProjectConstants.MaxNoteLength)).IsSuccess.Should().BeTrue();

    [Fact]
    public void Create_WithContentOverMaxLength_ShouldReturnNoteTooLong()
    {
        var result = ProjectNote.Create(new string('a', ProjectConstants.MaxNoteLength + 1));

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle().Which.Should().Be(ProjectError.NoteTooLong());
    }

    [Fact]
    public void Create_WithPaddingThatPushesContentOverMaxLength_ShouldReturnNoteTooLong()
        => ProjectNote.Create($" {new string('a', ProjectConstants.MaxNoteLength)}").Errors.Should().ContainSingle()
            .Which.Should().Be(ProjectError.NoteTooLong());
}
