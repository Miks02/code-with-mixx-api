using Microsoft.AspNetCore.Mvc;

namespace CodeWithMixx.API.Features.Students.ActivateStudent
{
    public record ActivateStudentRequest
    {
        [FromRoute(Name = "id")]
        public string Id { get; init; } = null!;
    };
}
