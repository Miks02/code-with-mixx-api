using CodeWithMixx.API.Common.Interfaces;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace CodeWithMixx.API.Infrastructure.Exceptions.Handlers;

public class RefreshTokenReuseExceptionHandler(
    ILogger<RefreshTokenReuseExceptionHandler> logger,
    IProblemDetailsService problemDetailsService) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not RefreshTokenReuseException ex)
            return false;

        var userIp = httpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown IP";
        var traceId = httpContext.TraceIdentifier;

        logger.LogWarning(
            "Security breach detected! Rotated refresh token reused, all user's sessions have been revoked. User ID: {UserId}, Session ID: {SessionId}, User IP: {UserIp}, TraceID: {TraceId}",
            ex.UserId, ex.SessionId, userIp, traceId);

        httpContext.RequestServices.GetRequiredService<ICookieProvider>().RemoveAuthCookies();
        httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Type = "urn:codewithmixx-api:error:Auth.SecurityBreach",
                Title = "Possible security breach",
                Status = StatusCodes.Status401Unauthorized,
                Detail = "All client sessions have been revoked due to possible security breach.",
                Instance = httpContext.Request.Path,
                Extensions = { ["errorCode"] = "Auth.SecurityBreach" }
            }
        });
    }
}
