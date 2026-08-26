using Microsoft.AspNetCore.Mvc;

namespace CodeWithMixx.API.Features.Subjects.DeleteSubject;

public record DeleteSubjectRequest
{
    [FromRoute(Name = "id")]
    public int Id { get; init; }
};
