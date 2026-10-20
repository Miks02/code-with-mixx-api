using CodeWithMixx.API.Common.Interfaces;

namespace CodeWithMixx.API.Features.Reservations.GetProjectReservationsSummary;

public class GetProjectReservationsSummaryEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/admin/reservations/projects/summary", async (
                [AsParameters] GetProjectReservationsSummaryRequest request,
                IHandler<GetProjectReservationsSummaryRequest, GetProjectReservationsSummaryResponse> handler,
                CancellationToken ct)
            => await handler.HandleAsync(request, ct))
        .RequireAuthorization("AdminOnly")
        .WithTags("Reservations")
        .Produces<GetProjectReservationsSummaryResponse>();
    }
}
