using Microsoft.AspNetCore.Mvc;

namespace CodeWithMixx.API.Features.Subjects.UpdateSubject;

public record UpdateSubjectRequest
{
    [FromRoute(Name = "id")]
    public int Id { get; init; }
    
    public string SubjectName { get; init; } = null!;
    public string SubjectDescription { get; init; } = null!;
};
