using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Common.Results;

namespace CodeWithMixx.API.Features.Reservations.UpdateProjectReservation;

public class UpdateProjectReservationEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("/admin/reservations/projects/{reservationId:int}", async (
                int reservationId,
                UpdateProjectReservationRequest request,
                IHandler<UpdateProjectReservationRequest, Result<UpdateProjectReservationResponse>> handler,
                CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(request with { Id = reservationId }, ct);
            return result.ToTypedResult();
        })
        .RequireAuthorization("AdminOnly")
        .WithTags("Reservations")
        .ProducesValidationProblem()
        .Produces<UpdateProjectReservationResponse>()
        .Produces(StatusCodes.Status404NotFound);
    }
}
