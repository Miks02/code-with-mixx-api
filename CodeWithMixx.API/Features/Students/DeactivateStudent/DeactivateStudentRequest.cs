using Microsoft.AspNetCore.Mvc;

namespace CodeWithMixx.API.Features.Students.DeactivateStudent
{
    public record DeactivateStudentRequest
    {
        [FromRoute(Name = "id")]
        public string Id { get; init; } = null!;
    };
}
