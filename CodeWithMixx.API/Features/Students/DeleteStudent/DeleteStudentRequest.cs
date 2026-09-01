using Microsoft.AspNetCore.Mvc;

namespace CodeWithMixx.API.Features.Students.DeleteStudent
{
    public class DeleteStudentRequest
    {
        [FromRoute(Name = "id")] 
        public string Id { get; init; } = null!;
    }
}
