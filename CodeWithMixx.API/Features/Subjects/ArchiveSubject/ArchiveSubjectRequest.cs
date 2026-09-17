using Microsoft.AspNetCore.Mvc;

namespace CodeWithMixx.API.Features.Subjects.ArchiveSubject;

public record ArchiveSubjectRequest
{
    [FromRoute(Name = "id")]
    public int Id { get; init; }
};
