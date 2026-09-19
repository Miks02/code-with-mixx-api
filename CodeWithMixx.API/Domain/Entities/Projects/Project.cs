using CodeWithMixx.API.Common.Results;
using CodeWithMixx.API.Domain.Entities.Reservations;
using CodeWithMixx.API.Domain.Entities.Subjects;

namespace CodeWithMixx.API.Domain.Entities.Projects;

public class Project : IAuditable, ISoftDeletable
{
    public int Id { get; private set; }
    public ProjectType ProjectType { get; private set; }
    public decimal Price { get; private set; }
    public string? GithubLink { get; private set; }
    public string? DownloadLink { get; private set; }
    public decimal Progress { get; private set; }
    
    public DateTime ReservedAt { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }

    public Subject Subject { get; private set; } = null!;
    public int SubjectId { get; private set; }

    public Reservation Reservation { get; private set; } = null!;
    public int ReservationId { get; private set; }
    
    private readonly List<ProjectNote> _projectNotes = [];
    public IReadOnlyCollection<ProjectNote> ProjectNotes => _projectNotes.AsReadOnly();

    public static Result<Project> Create(ProjectCreateData data)
    {
        var validationResult = Validate(data.SubjectId, data.Price, data.Progress, data.StartDate, data.EndDate,
            data.ReservedAt, data.GithubLink, data.DownloadLink);

        if(!validationResult.IsSuccess)
            return Result<Project>.Failure(validationResult.Errors[0]);

        var newProject = new Project
        {
            SubjectId = data.SubjectId,
            Price = data.Price,
            ProjectType = data.ProjectType,
            GithubLink = data.GithubLink,
            DownloadLink = data.DownloadLink,
            Progress = data.Progress,
            ReservedAt = data.ReservedAt,
            StartDate = data.StartDate,
            EndDate = data.EndDate,
            CreatedAt = DateTime.UtcNow
        };

        foreach (var note in data.Notes)
        {
            var noteResult = ProjectNote.Create(note.Content);
            if(!noteResult.IsSuccess)
                return Result<Project>.Failure(noteResult.Errors[0]);
            
            newProject._projectNotes.Add(noteResult.Payload!);
        }
        
        return Result<Project>.Success(newProject);   
    }
    
    public Result Update(ProjectUpdateData data)
    {
        var validationResult = Validate(data.SubjectId, data.Price, data.Progress, data.StartDate, data.EndDate,
            data.ReservedAt, data.GithubLink, data.DownloadLink);

        if(!validationResult.IsSuccess)
            return validationResult;

        SubjectId = data.SubjectId;
        ProjectType = data.ProjectType;
        Price = data.Price;
        Progress = data.Progress;
        ReservedAt = data.ReservedAt;
        StartDate = data.StartDate;
        EndDate = data.EndDate;
        GithubLink = data.GithubLink;
        DownloadLink = data.DownloadLink;
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }

    public Result Delete()
    {
        if(DeletedAt is not null)
            return Result.Failure(ProjectError.AlreadyDeleted(Id));
        
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
        return Result.Success();
    }

    public Result Restore()
    {
        if(!IsDeleted)
            return Result.Failure(ProjectError.NotArchived(Id));

        IsDeleted = false;
        DeletedAt = null;
        return Result.Success();
    }

    private static Result Validate(int subjectId, decimal price, decimal progress, DateTime startDate, DateTime endDate,
        DateTime reservedAt, string? githubLink, string? downloadLink)
    {
        if(subjectId <= 0)
            return Result.Failure(SubjectError.NotFound(subjectId));

        if(price < 0)
            return Result.Failure(ProjectError.NegativePrice());

        if(progress < 0 || progress > 100)
            return Result.Failure(ProjectError.InvalidProgress(progress));

        if(startDate <= DateTime.MinValue || startDate > endDate)
            return Result.Failure(ProjectError.InvalidDateRange(startDate, endDate));

        if(reservedAt <= DateTime.MinValue || reservedAt > DateTime.UtcNow)
            return Result.Failure(ProjectError.InvalidReservationDate(reservedAt));

        if(githubLink is not null && githubLink.Length > 200)
            return Result.Failure(ProjectError.GithubLinkIsTooLong(githubLink));

        if(downloadLink is not null && downloadLink.Length > 200)
            return Result.Failure(ProjectError.DownloadLinkIsTooLong(downloadLink));

        return Result.Success();
    }
}