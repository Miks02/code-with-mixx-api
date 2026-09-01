using Microsoft.AspNetCore.Mvc;

namespace CodeWithMixx.API.Features.Students.GetStudentById;

public record GetStudentByIdRequest
{
    [FromRoute(Name = "id")]
    public string Id { get; init; } = null!;
};
