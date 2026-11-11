using CodeWithMixx.API.Common.Interfaces;

namespace CodeWithMixx.API.Features.Analytics.GetAnalyticsForAdmin;

public class GetAnalyticsForAdminEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/admin/analytics", async (
                [AsParameters] GetAnalyticsForAdminRequest request,
                IHandler<GetAnalyticsForAdminRequest, GetAnalyticsForAdminResponse> handler) =>
        {
            var response = await handler.HandleAsync(request);
            return Results.Ok(response);
        })
        .RequireAuthorization("AdminOnly")
        .WithTags("Analytics")
        .Produces<GetAnalyticsForAdminResponse>()
        .Produces(StatusCodes.Status401Unauthorized);
    }
}