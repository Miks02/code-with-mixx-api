using CodeWithMixx.API.Common.Interfaces;

namespace CodeWithMixx.API.Features.Analytics.GetAnalyticsForStudent;

public class GetAnalyticsForStudentEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/student/analytics", async (
                IHandler<GetAnalyticsForStudentRequest, GetAnalyticsForStudentResponse> handler,
                CancellationToken ct) =>
        {
            var response = await handler.HandleAsync(new GetAnalyticsForStudentRequest(), ct);
            return Results.Ok(response);
        })
        .RequireAuthorization("StudentOnly")
        .WithTags("Analytics")
        .Produces<GetAnalyticsForStudentResponse>()
        .Produces(StatusCodes.Status401Unauthorized);
    }
}
