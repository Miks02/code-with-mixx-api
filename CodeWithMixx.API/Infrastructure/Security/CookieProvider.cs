using System.Net;
using CodeWithMixx.API.Features.Authentication.Common;

namespace CodeWithMixx.API.Infrastructure.Security;

public class CookieProvider(IConfiguration configuration, IHttpContextAccessor http) : ICookieProvider
{
    private HttpContext Context => http.HttpContext!;

    public string? GetRefreshTokenCookie() => Context.Request.Cookies["RefreshToken"];
    public string? GetAccessTokenCookie() => Context.Request.Cookies["AccessToken"];

    public void SetAccessTokenCookie(string token) =>
        AppendCookie("AccessToken", token, DateTimeOffset.UtcNow.AddMinutes(configuration.GetValue<int>("JwtConfig:ExpirationInMinutes")));

    public void SetRefreshTokenCookie(string token) =>
        AppendCookie("RefreshToken", token, DateTimeOffset.UtcNow.AddDays(configuration.GetValue<int>("RefreshConfig:ExpirationInDays")));

    public void SetSessionCookie()
    {
        Context.Response.Cookies.Append("Session", "", new CookieOptions
        {
            Path = "/",
            HttpOnly = false,
            Secure = true,
            SameSite = SameSiteMode.Lax,
            Expires = DateTimeOffset.UtcNow.AddDays(configuration.GetValue<int>("RefreshConfig:ExpirationInDays"))
        });
    }

    public void RemoveAuthCookies()
    {
        Context.Response.Cookies.Delete("AccessToken");
        Context.Response.Cookies.Delete("RefreshToken");
        Context.Response.Cookies.Delete("Session");
    }

    private void AppendCookie(string name, string value, DateTimeOffset expires)
    {
        Context.Response.Cookies.Append(name, value, GetCookieOptions(expires));
    }
    
    private CookieOptions GetCookieOptions(DateTimeOffset expires)
    {
        var domain = configuration.GetValue<string?>("CookieConfig:Domain");

        return new CookieOptions
        {
            Path = "/",
            Domain = domain,
            HttpOnly = true,
            Secure = true,
            IsEssential = true,
            SameSite = SameSiteMode.Lax,
            Expires = expires
        };
    }
}