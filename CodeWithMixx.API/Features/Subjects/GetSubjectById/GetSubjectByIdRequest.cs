using Microsoft.AspNetCore.Mvc;

namespace CodeWithMixx.API.Features.Subjects.GetSubjectById;

public record GetSubjectByIdRequest
{
    [FromRoute(Name = "id")]
    public int Id { get; init; }
};