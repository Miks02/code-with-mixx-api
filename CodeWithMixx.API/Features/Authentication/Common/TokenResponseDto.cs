
namespace CodeWithMixx.API.Features.Authentication.Common;

public record TokenResponseDto
{
    public string AccessToken { get; init; } = null!;
    public string RefreshToken { get; init; } = null!;
};