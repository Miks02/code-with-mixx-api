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
    
    
}