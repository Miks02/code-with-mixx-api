using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Common.Results;

namespace CodeWithMixx.API.Features.Classes.UpdateClassReservation;

public class UpdateClassReservationEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("classes/{reservationId:int}", async (
                int reservationId,
                UpdateClassReservationRequest request,
                IHandler<UpdateClassReservationRequest, Result<UpdateClassReservationResponse>> handler,
                CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(request with { Id = reservationId }, ct);
            return result.ToTypedResult();
        })
        .RequireAuthorization("AdminOnly")
        .WithTags("Classes")
        .ProducesValidationProblem()
        .Produces<UpdateClassReservationResponse>()
        .Produces(StatusCodes.Status404NotFound);
    }
}
