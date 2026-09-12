namespace CodeWithMixx.API.Features.Authentication.Login;

public record LoginResponse
{
    public string FirstName { get; init; } = null!;
    public string LastName { get; init; } = null!;
    public string Email { get; init; } = null!;
    public string PhoneNumber { get; init; } = null!;
    public IReadOnlyList<string> Roles { get; init; } = [];
};