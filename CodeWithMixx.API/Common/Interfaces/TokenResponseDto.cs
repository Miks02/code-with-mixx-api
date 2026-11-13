
namespace CodeWithMixx.API.Common.Interfaces;

public record TokenResponseDto
{
    public string AccessToken { get; init; } = null!;
    public string RefreshToken { get; init; } = null!;
};