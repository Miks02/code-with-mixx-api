namespace CodeWithMixx.API.Features.Students.GetStudentsLookup;

public record GetStudentsLookupResponse
{
    public string Id { get; init; } = null!;
    public string FullName { get; init; } = null!;
    public string Email { get; init; } = null!;
};
