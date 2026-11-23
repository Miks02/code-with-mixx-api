using AwesomeAssertions;
using CodeWithMixx.API.Common.Result;
using CodeWithMixx.API.Domain.Entities.Projects;
using CodeWithMixx.API.Domain.Entities.Subjects;
using Xunit;

namespace CodeWithMixx.UnitTests.Domain.Entities.Projects;

public class ProjectTests
{
    private const int SubjectId = 3;
    private const decimal Price = 25000m;
    private const decimal Progress = 40m;
    private const string GithubLink = "https://github.com/mixx/seminarski";
    private const string DownloadLink = "https://drive.example.com/seminarski.zip";
    private const string FirstNote = "Klijent želi ASP.NET backend.";
    private const string SecondNote = "Rok za prvu verziju je kraj meseca.";
    private const string ProjectName = "Web aplikacija izradjena u ASP.NET Core MVC";

    private static readonly DateOnly Today = new(2026, 11, 15);
    private static readonly DateOnly ReservedAt = new(2026, 11, 1);
    private static readonly DateOnly StartDate = new(2026, 11, 10);
    private static readonly DateOnly EndDate = new(2026, 12, 10);

    private readonly Project _sut;

    public ProjectTests()
    {
        _sut = Project.Create(CreateData()).Payload!;
    }

    [Fact]
    public void Create_WithValidData_ShouldSucceedAndMapValues()
    {
        var result = Project.Create(CreateData());

        result.IsSuccess.Should().BeTrue();
        var project = result.Payload!;
        project.SubjectId.Should().Be(SubjectId);
        project.Name.Should().Be(ProjectName);
        project.ProjectType.Should().Be(ProjectType.Seminar);
        project.Price.Should().Be(Price);
        project.Progress.Should().Be(Progress);
        project.ReservedAt.Should().Be(ReservedAt);
        project.StartDate.Should().Be(StartDate);
        project.EndDate.Should().Be(EndDate);
        project.GithubLink.Should().Be(GithubLink);
        project.DownloadLink.Should().Be(DownloadLink);
    }

    [Fact]
    public void Create_ShouldStartNotDeletedWithUnsetIdAndAuditFields()
    {
        _sut.Id.Should().Be(0);
        _sut.ReservationId.Should().Be(0);
        _sut.CreatedAt.Should().Be(default);
        _sut.UpdatedAt.Should().BeNull();
        _sut.IsDeleted.Should().BeFalse();
        _sut.DeletedAt.Should().BeNull();
    }

    [Fact]
    public void Create_WithoutNotes_ShouldHaveNoNotes()
        => _sut.ProjectNotes.Should().BeEmpty();

    [Fact]
    public void Create_WithNotes_ShouldAddNotesInOrder()
    {
        var project = Project.Create(CreateData(notes: [FirstNote, SecondNote])).Payload!;

        project.ProjectNotes.Select(n => n.Content).Should().Equal(FirstNote, SecondNote);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithBlankNote_ShouldReturnEmptyProjectNote(string note)
        => AssertFails(Project.Create(CreateData(notes: [FirstNote, note])), ProjectError.EmptyProjectNote());

    [Theory]
    [InlineData(ProjectType.Seminar)]
    [InlineData(ProjectType.Graduation)]
    public void Create_WithEveryDefinedProjectType_ShouldSucceed(ProjectType type)
        => Project.Create(CreateData() with { ProjectType = type }).Payload!.ProjectType.Should().Be(type);

    [Fact]
    public void Create_WithoutLinks_ShouldSucceedWithNullLinks()
    {
        var result = Project.Create(CreateData() with { GithubLink = null, DownloadLink = null });

        result.IsSuccess.Should().BeTrue();
        result.Payload!.GithubLink.Should().BeNull();
        result.Payload.DownloadLink.Should().BeNull();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_WithNonPositiveSubjectId_ShouldReturnSubjectNotFound(int subjectId)
        => AssertFails(Project.Create(CreateData() with { SubjectId = subjectId }), SubjectError.NotFound(subjectId));

    [Fact]
    public void Create_WithZeroPrice_ShouldSucceed()
        => Project.Create(CreateData() with { Price = 0 }).IsSuccess.Should().BeTrue();

    [Fact]
    public void Create_WithNegativePrice_ShouldReturnNegativePrice()
        => AssertFails(Project.Create(CreateData() with { Price = -0.01m }), ProjectError.NegativePrice());

    [Theory]
    [InlineData(0)]
    [InlineData(100)]
    [InlineData(33.33)]
    public void Create_WithProgressInRange_ShouldSucceed(decimal progress)
        => Project.Create(CreateData() with { Progress = progress }).IsSuccess.Should().BeTrue();

    [Theory]
    [InlineData(-0.01)]
    [InlineData(100.01)]
    public void Create_WithProgressOutOfRange_ShouldReturnInvalidProgress(decimal progress)
        => AssertFails(Project.Create(CreateData() with { Progress = progress }), ProjectError.InvalidProgress(progress));

    [Fact]
    public void Create_WithStartDateAfterEndDate_ShouldReturnInvalidDateRange()
    {
        var data = CreateData() with { StartDate = EndDate.AddDays(1) };

        AssertFails(Project.Create(data), ProjectError.InvalidDateRange(data.StartDate, data.EndDate));
    }

    [Fact]
    public void Create_WithStartDateEqualToEndDate_ShouldSucceed()
        => Project.Create(CreateData() with { EndDate = StartDate }).IsSuccess.Should().BeTrue();

    [Fact]
    public void Create_WithDefaultStartDate_ShouldReturnStartDateTooEarly()
        => AssertFails(Project.Create(CreateData() with { StartDate = default }), ProjectError.StartDateTooEarly(default));

    [Fact]
    public void Create_WithStartDateAtMinStartDate_ShouldReturnStartDateTooEarly()
    {
        var data = CreateData() with { ReservedAt = ProjectConstants.MinStartDate, StartDate = ProjectConstants.MinStartDate };

        AssertFails(Project.Create(data), ProjectError.StartDateTooEarly(ProjectConstants.MinStartDate));
    }

    [Fact]
    public void Create_WithStartDateDayAfterMinStartDate_ShouldSucceed()
    {
        var data = CreateData() with { ReservedAt = ProjectConstants.MinStartDate, StartDate = ProjectConstants.MinStartDate.AddDays(1) };

        Project.Create(data).IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Create_WithStartDateTooEarlyAndAfterEndDate_ShouldReportStartDateTooEarlyFirst()
    {
        var data = CreateData() with { StartDate = new DateOnly(1999, 6, 1), EndDate = new DateOnly(1999, 1, 1) };

        AssertFails(Project.Create(data), ProjectError.StartDateTooEarly(data.StartDate));
    }

    [Fact]
    public void Create_WithDefaultReservedAt_ShouldReturnInvalidReservationDate()
        => AssertFails(Project.Create(CreateData() with { ReservedAt = default }), ProjectError.InvalidReservationDate(default));

    [Fact]
    public void Create_WithReservedAtInTheFuture_ShouldReturnInvalidReservationDate()
    {
        var reservedAt = Today.AddDays(1);
        var data = CreateData() with { ReservedAt = reservedAt, StartDate = reservedAt, EndDate = reservedAt.AddDays(10) };

        AssertFails(Project.Create(data), ProjectError.InvalidReservationDate(reservedAt));
    }

    [Fact]
    public void Create_WithReservedAtToday_ShouldSucceed()
        => Project.Create(CreateData() with { ReservedAt = Today, StartDate = Today }).IsSuccess.Should().BeTrue();

    [Fact]
    public void Create_WithStartDateBeforeReservedAt_ShouldReturnStartDateBeforeReservedAt()
    {
        var startDate = ReservedAt.AddDays(-1);

        AssertFails(Project.Create(CreateData() with { StartDate = startDate }), ProjectError.StartDateBeforeReservedAt(startDate, ReservedAt));
    }

    [Fact]
    public void Create_WithStartDateEqualToReservedAt_ShouldSucceed()
        => Project.Create(CreateData() with { StartDate = ReservedAt }).IsSuccess.Should().BeTrue();

    [Fact]
    public void Create_WithStartDateInThePast_ShouldSucceed()
    {
        var data = CreateData() with { ReservedAt = new DateOnly(2025, 1, 1), StartDate = new DateOnly(2025, 1, 5), EndDate = new DateOnly(2025, 2, 1) };

        Project.Create(data).IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Create_WithGithubLinkAtMaxLength_ShouldSucceed()
        => Project.Create(CreateData() with { GithubLink = new string('a', ProjectConstants.MaxLinkLength) }).IsSuccess.Should().BeTrue();

    [Fact]
    public void Create_WithGithubLinkOverMaxLength_ShouldReturnGithubLinkIsTooLong()
    {
        var link = new string('a', ProjectConstants.MaxLinkLength + 1);

        AssertFails(Project.Create(CreateData() with { GithubLink = link }), ProjectError.GithubLinkIsTooLong(link));
    }

    [Fact]
    public void Create_WithDownloadLinkAtMaxLength_ShouldSucceed()
        => Project.Create(CreateData() with { DownloadLink = new string('a', ProjectConstants.MaxLinkLength) }).IsSuccess.Should().BeTrue();

    [Fact]
    public void Create_WithDownloadLinkOverMaxLength_ShouldReturnDownloadLinkIsTooLong()
    {
        var link = new string('a', ProjectConstants.MaxLinkLength + 1);

        AssertFails(Project.Create(CreateData() with { DownloadLink = link }), ProjectError.DownloadLinkIsTooLong(link));
    }

    [Fact]
    public void Create_WithMultipleInvalidValues_ShouldReportSubjectFirst()
    {
        var data = CreateData(notes: [""]) with { SubjectId = 0, Price = -1, Progress = 200, GithubLink = new string('a', ProjectConstants.MaxLinkLength + 1) };

        AssertFails(Project.Create(data), SubjectError.NotFound(0));
    }

    [Fact]
    public void Create_WithInvalidFieldAndBlankNote_ShouldReportFieldBeforeNote()
        => AssertFails(Project.Create(CreateData(notes: [""]) with { Price = -1 }), ProjectError.NegativePrice());

    [Fact]
    public void Create_WithStartDateBeforeMinStartDate_ShouldReturnStartDateTooEarly()
    {
        var data = CreateData() with { ReservedAt = new DateOnly(1999, 1, 1), StartDate = new DateOnly(1999, 1, 2), EndDate = new DateOnly(1999, 2, 1) };

        AssertFails(Project.Create(data), ProjectError.StartDateTooEarly(data.StartDate));
    }

    [Fact]
    public void Create_WithNoteOverMaxLength_ShouldReturnNoteTooLong()
        => AssertFails(Project.Create(CreateData(notes: [FirstNote, new string('a', ProjectConstants.MaxNoteLength + 1)])), ProjectError.NoteTooLong());

    [Theory]
    [InlineData(-1)]
    [InlineData(2)]
    public void Create_WithUndefinedProjectType_ShouldCurrentlyBeAccepted(int type)
        => Project.Create(CreateData() with { ProjectType = (ProjectType)type }).IsSuccess.Should().BeTrue();

    [Fact]
    public void Update_WithValidData_ShouldSucceedAndReplaceValues()
    {
        var data = UpdateData() with
        {
            SubjectId = 9,
            ProjectType = ProjectType.Graduation,
            Price = 40000m,
            Progress = 100m,
            ReservedAt = new DateOnly(2026, 11, 2),
            StartDate = new DateOnly(2026, 11, 3),
            EndDate = new DateOnly(2027, 1, 15),
            GithubLink = null,
            DownloadLink = "https://drive.example.com/diplomski.zip"
        };

        var result = _sut.Update(data);

        result.IsSuccess.Should().BeTrue();
        _sut.SubjectId.Should().Be(9);
        _sut.ProjectType.Should().Be(ProjectType.Graduation);
        _sut.Price.Should().Be(40000m);
        _sut.Progress.Should().Be(100m);
        _sut.ReservedAt.Should().Be(new DateOnly(2026, 11, 2));
        _sut.StartDate.Should().Be(new DateOnly(2026, 11, 3));
        _sut.EndDate.Should().Be(new DateOnly(2027, 1, 15));
        _sut.GithubLink.Should().BeNull();
        _sut.DownloadLink.Should().Be("https://drive.example.com/diplomski.zip");
    }

    [Fact]
    public void Update_WithInvalidValue_ShouldReturnErrorAndKeepValues()
    {
        var result = _sut.Update(UpdateData() with { Price = -1, Progress = 90m });

        AssertFails(result, ProjectError.NegativePrice());
        _sut.Price.Should().Be(Price);
        _sut.Progress.Should().Be(Progress);
    }

    [Fact]
    public void Update_WithProgressOutOfRange_ShouldReturnInvalidProgress()
        => AssertFails(_sut.Update(UpdateData() with { Progress = 101 }), ProjectError.InvalidProgress(101));

    [Fact]
    public void Update_WithReservedAtInTheFuture_ShouldReturnInvalidReservationDate()
    {
        var reservedAt = Today.AddDays(1);

        AssertFails(_sut.Update(UpdateData() with { ReservedAt = reservedAt, StartDate = reservedAt }), ProjectError.InvalidReservationDate(reservedAt));
    }

    [Fact]
    public void Update_WithStartDateBeforeMinStartDate_ShouldReturnStartDateTooEarlyAndKeepValues()
    {
        var startDate = new DateOnly(1999, 12, 31);

        var result = _sut.Update(UpdateData() with { ReservedAt = new DateOnly(1999, 12, 1), StartDate = startDate });

        AssertFails(result, ProjectError.StartDateTooEarly(startDate));
        _sut.StartDate.Should().Be(StartDate);
        _sut.ReservedAt.Should().Be(ReservedAt);
    }

    [Fact]
    public void Update_WithNewNoteOverMaxLength_ShouldReturnNoteTooLongAndKeepNotes()
    {
        var project = CreateWithPersistedNotes(FirstNote);

        var result = project.Update(UpdateData() with { Notes = [Note(0, new string('a', ProjectConstants.MaxNoteLength + 1))] });

        AssertFails(result, ProjectError.NoteTooLong());
        project.ProjectNotes.Select(n => n.Content).Should().Equal(FirstNote);
    }

    [Fact]
    public void Update_WithNewNotes_ShouldAppendThem()
    {
        var project = CreateWithPersistedNotes(FirstNote);

        var result = project.Update(UpdateData() with { Notes = [Note(0, SecondNote)] });

        result.IsSuccess.Should().BeTrue();
        project.ProjectNotes.Select(n => n.Content).Should().Equal(FirstNote, SecondNote);
    }

    [Fact]
    public void Update_WithExistingNoteIdsInNotes_ShouldNotDuplicateOrChangeThem()
    {
        var project = CreateWithPersistedNotes(FirstNote);

        project.Update(UpdateData() with { Notes = [Note(1, "Izmenjen sadržaj")] });

        project.ProjectNotes.Should().ContainSingle().Which.Content.Should().Be(FirstNote);
    }

    [Fact]
    public void Update_WithNotesToDelete_ShouldRemoveThem()
    {
        var project = CreateWithPersistedNotes(FirstNote, SecondNote);

        var result = project.Update(UpdateData() with { NotesToDelete = [1] });

        result.IsSuccess.Should().BeTrue();
        project.ProjectNotes.Should().ContainSingle().Which.Id.Should().Be(2);
    }

    [Fact]
    public void Update_WithNotesToDeleteAndNewNotes_ShouldRemoveAndAppend()
    {
        var project = CreateWithPersistedNotes(FirstNote, SecondNote);

        project.Update(UpdateData() with { NotesToDelete = [1, 2], Notes = [Note(0, "Nova napomena")] });

        project.ProjectNotes.Should().ContainSingle().Which.Content.Should().Be("Nova napomena");
    }

    [Fact]
    public void Update_WithDuplicateNotesToDelete_ShouldRemoveNoteOnce()
    {
        var project = CreateWithPersistedNotes(FirstNote, SecondNote);

        project.Update(UpdateData() with { NotesToDelete = [1, 1] }).IsSuccess.Should().BeTrue();

        project.ProjectNotes.Select(n => n.Id).Should().Equal(2);
    }

    [Fact]
    public void Update_WithUnknownNoteToDelete_ShouldReturnNoteNotFoundAndKeepNotes()
    {
        var project = CreateWithPersistedNotes(FirstNote);

        var result = project.Update(UpdateData() with { NotesToDelete = [1, 99], Notes = [Note(0, SecondNote)] });

        AssertFails(result, ProjectError.NoteNotFound(99));
        project.ProjectNotes.Select(n => n.Id).Should().Equal(1);
    }

    [Fact]
    public void Update_WithBlankNewNote_ShouldReturnEmptyProjectNoteAndKeepNotes()
    {
        var project = CreateWithPersistedNotes(FirstNote);

        var result = project.Update(UpdateData() with { NotesToDelete = [1], Notes = [Note(0, SecondNote), Note(0, " ")] });

        AssertFails(result, ProjectError.EmptyProjectNote());
        project.ProjectNotes.Select(n => n.Content).Should().Equal(FirstNote);
    }

    [Fact]
    public void Update_WithUnknownNoteToDelete_ShouldCurrentlyStillApplyFieldChanges()
    {
        var result = _sut.Update(UpdateData() with { Price = 1m, NotesToDelete = [99] });

        result.IsSuccess.Should().BeFalse();
        _sut.Price.Should().Be(1m);
    }

    [Fact]
    public void Update_WithBlankNewNote_ShouldCurrentlyStillApplyFieldChanges()
    {
        var result = _sut.Update(UpdateData() with { Progress = 99m, Notes = [Note(0, "")] });

        result.IsSuccess.Should().BeFalse();
        _sut.Progress.Should().Be(99m);
    }

    [Fact]
    public void Update_WhenDeleted_ShouldCurrentlySucceed()
    {
        _sut.Delete();

        _sut.Update(UpdateData() with { Price = 1m }).IsSuccess.Should().BeTrue();
        _sut.Price.Should().Be(1m);
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
    public void Delete_WhenAlreadyDeleted_ShouldReturnAlreadyDeletedAndKeepOriginalDeletedAt()
    {
        _sut.Delete();
        var firstDeletedAt = _sut.DeletedAt;

        var result = _sut.Delete();

        AssertFails(result, ProjectError.AlreadyDeleted(_sut.Id));
        _sut.DeletedAt.Should().Be(firstDeletedAt);
    }

    [Fact]
    public void Delete_ShouldKeepNotes()
    {
        var project = CreateWithPersistedNotes(FirstNote);

        project.Delete();

        project.ProjectNotes.Should().ContainSingle();
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

        AssertFails(result, ProjectError.NotArchived(_sut.Id));
        _sut.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public void Delete_AfterRestore_ShouldSucceed()
    {
        _sut.Delete();
        _sut.Restore();

        _sut.Delete().IsSuccess.Should().BeTrue();
        _sut.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public void ProjectNotes_ShouldNotBeMutableList()
        => _sut.ProjectNotes.Should().NotBeAssignableTo<List<ProjectNote>>();

    private static ProjectCreateData CreateData(params string[] notes) => new()
    {
        SubjectId = SubjectId,
        Name = ProjectName,
        ProjectType = ProjectType.Seminar,
        ReservedAt = ReservedAt,
        StartDate = StartDate,
        EndDate = EndDate,
        GithubLink = GithubLink,
        DownloadLink = DownloadLink,
        Price = Price,
        Progress = Progress,
        Notes = notes.Select(n => new ProjectCreateData.ProjectNoteData { Content = n }).ToList(),
        Today = Today
    };

    private static ProjectUpdateData UpdateData() => new()
    {
        SubjectId = SubjectId,
        Name = ProjectName,
        ProjectType = ProjectType.Seminar,
        ReservedAt = ReservedAt,
        StartDate = StartDate,
        EndDate = EndDate,
        GithubLink = GithubLink,
        DownloadLink = DownloadLink,
        Price = Price,
        Progress = Progress,
        Today = Today
    };

    private static ProjectUpdateData.ProjectNoteData Note(int id, string content)
        => new() { Id = id, Content = content };

    private static Project CreateWithPersistedNotes(params string[] notes)
    {
        var project = Project.Create(CreateData(notes)).Payload!;
        var id = 1;
        foreach (var note in project.ProjectNotes)
            typeof(ProjectNote).GetProperty(nameof(ProjectNote.Id))!.SetValue(note, id++);

        return project;
    }

    private static void AssertFails(API.Common.Results.Result result, Error expected)
    {
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle().Which.Should().Be(expected);
    }
}
