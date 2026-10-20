using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Common.Results;

namespace CodeWithMixx.API.Features.Reservations.GetPagedProjectReservations;

public class GetPagedProjectReservationsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/admin/reservations/projects", async (
                [AsParameters] GetPagedProjectReservationsRequest request,
                IHandler<GetPagedProjectReservationsRequest, PagedResult<GetPagedProjectReservationsResponse>> handler,
                CancellationToken ct)
            => await handler.HandleAsync(request, ct))
        .RequireAuthorization("AdminOnly")
        .WithTags("Reservations")
        .Produces<PagedResult<GetPagedProjectReservationsResponse>>();
    }
}
