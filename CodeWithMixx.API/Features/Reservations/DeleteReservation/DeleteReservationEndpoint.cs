using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Common.Results;

namespace CodeWithMixx.API.Features.Reservations.DeleteReservation;

public class DeleteReservationEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("/admin/reservations/{reservationId:int}", async (
            int reservationId,
            IHandler<DeleteReservationRequest, Result> handler,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(new DeleteReservationRequest(reservationId), ct);
            return result.ToTypedResult();
        })
        .RequireAuthorization("AdminOnly")
        .WithTags("Reservations");
    }
}