using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace CodeWithMixx.API.Infrastructure.Exceptions.Handlers;

public class TokensRevokedExceptionHandler(
    ILogger<TokensRevokedExceptionHandler> logger,
    IProblemDetailsService problemDetailsService,
    IConfiguration configuration) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not TokensRevokedException ex)
            return false;
        
        var userIp = httpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown IP";
        var traceId = httpContext.TraceIdentifier;
        
        logger.LogWarning("Security breach detected! All user's sessions have been revoked. User ID: {UserId} . User IP address: {UserIp} . TraceID: {TraceId}"
            , ex.UserId, userIp, traceId);
        
        httpContext.Response.Cookies.Delete("RefreshToken", new CookieOptions
        {
            IsEssential = true,
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax,
            Path = "/",
            Domain = configuration["CookieConfig:Domain"]
        });
        
        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Type = "urn:code-with-mixx-api:error:AuthError.SecurityBreach",
                Title = "Possible security breach",
                Status = StatusCodes.Status401Unauthorized,
                Detail = "All client sessions have been revoked due to possible security breach.",
                Instance = httpContext.Request.Path
            }
        });
        
    }
}