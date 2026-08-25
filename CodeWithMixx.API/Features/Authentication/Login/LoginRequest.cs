using System.ComponentModel.DataAnnotations;

namespace CodeWithMixx.API.Features.Authentication.Login;

public record LoginRequest
{
    public string Email { get; init; } = null!;
    public string Password { get; init; } = null!;
};