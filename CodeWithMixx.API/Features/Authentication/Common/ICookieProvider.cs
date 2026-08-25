namespace CodeWithMixx.API.Features.Authentication.Common;

public interface ICookieProvider
{
    string? GetRefreshTokenCookie();
    string? GetAccessTokenCookie();
    void SetRefreshTokenCookie(string refreshToken);
    void SetAccessTokenCookie(string accessToken);
    void RemoveAuthCookies();
}