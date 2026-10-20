using System.Net;
using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Common.Results;

namespace CodeWithMixx.API.Features.Reservations.UpdateReservationStatus;

public class UpdateReservationStatusEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPatch("/admin/reservations/{reservationId:int}/status", async (
                int reservationId,
                UpdateReservationStatusBody request,
                IHandler<UpdateReservationStatusRequest, Result> handler,
                CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(new UpdateReservationStatusRequest
            {
                ReservationId = reservationId,
                Body = request
            }, ct);

            return result.ToTypedResult(HttpStatusCode.NoContent);
        })
        .RequireAuthorization("AdminOnly")
        .WithTags("Reservations")
        .ProducesValidationProblem()
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status409Conflict);
    }
}
