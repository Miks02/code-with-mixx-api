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
        if(data.SubjectId <= 0)
            return Result<Project>.Failure(SubjectError.NotFound(data.SubjectId)); 
        
        if(data.Price < 0)
            return Result<Project>.Failure(ProjectError.NegativePrice());
        
        if(data.Progress < 0 || data.Progress > 100)
            return Result<Project>.Failure(ProjectError.InvalidProgress(data.Progress));
        
        if(data.StartDate <= DateTime.MinValue || data.StartDate > data.EndDate)
            return Result<Project>.Failure(ProjectError.InvalidDateRange(data.StartDate, data.EndDate));
        
        if(data.ReservedAt <= DateTime.MinValue || data.ReservedAt > DateTime.UtcNow)
            return Result<Project>.Failure(ProjectError.InvalidReservationDate(data.ReservedAt));
        
        if(data.GithubLink is not null && data.GithubLink.Length > 200)
            return Result<Project>.Failure(ProjectError.GithubLinkIsTooLong(data.GithubLink));
        
        if(data.DownloadLink is not null && data.DownloadLink.Length > 200)
            return Result<Project>.Failure(ProjectError.DownloadLinkIsTooLong(data.DownloadLink));

        var newProject = new Project
        {
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
    
    public Result Delete()
    {
        if(DeletedAt is not null)
            return Result.Failure(ProjectError.AlreadyDeleted(Id));
        
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
        return Result.Success();
    }
    
    
}