using System.Net;
using CodeWithMixx.API.Features.Authentication.Common;

namespace CodeWithMixx.API.Infrastructure.Security;

public class CookieProvider(IConfiguration configuration, IHttpContextAccessor http) : ICookieProvider
{
    private HttpContext Context => http.HttpContext!;

    public string? GetRefreshTokenCookie() => Context.Request.Cookies["RefreshToken"];
    public string? GetAccessTokenCookie() => Context.Request.Cookies["AccessToken"];

    public void SetAccessTokenCookie(string token) =>
        AppendCookie("AccessToken", token, DateTime.UtcNow.AddMinutes(configuration.GetValue<int>("JwtConfig:ExpirationInMinutes")));

    public void SetRefreshTokenCookie(string token) =>
        AppendCookie("RefreshToken", token, DateTime.UtcNow.AddDays(configuration.GetValue<int>("RefreshConfig:ExpirationInDays")));

    public void RemoveAuthCookies()
    {
        Context.Response.Cookies.Delete("AccessToken");
        Context.Response.Cookies.Delete("RefreshToken");
    }

    private void AppendCookie(string name, string value, DateTime expires)
    {
        Context.Response.Cookies.Append(name, value, GetCookieOptions(expires));
    }
    
    private CookieOptions GetCookieOptions(DateTime expires)
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