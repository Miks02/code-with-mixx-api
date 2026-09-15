using Microsoft.AspNetCore.Mvc;

namespace CodeWithMixx.API.Features.Subjects.RestoreSubject;

public record RestoreSubjectRequest
{
    [FromRoute(Name = "id")]
    public int Id { get; init; }
};
