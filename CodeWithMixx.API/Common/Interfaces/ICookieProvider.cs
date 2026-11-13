namespace CodeWithMixx.API.Common.Interfaces;

public interface ICookieProvider
{
    string? GetRefreshTokenCookie();
    string? GetAccessTokenCookie();
    void SetRefreshTokenCookie(string refreshToken);
    void SetAccessTokenCookie(string accessToken);
    void SetSessionCookie();
    void RemoveAuthCookies();
}