namespace CodeWithMixx.API.Features.Students.SearchStudents
{
    public record SearchStudentsRequest
    {
        public string Search { get; set; } = null!;
    }
}
