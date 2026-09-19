using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Common.Results;

namespace CodeWithMixx.API.Features.Reservations.SubtractPayment;

public class SubtractPaymentEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/admin/reservations/{reservationId:int}/payments/subtract", async (
                int reservationId,
                SubtractPaymentBody request,
                IHandler<SubtractPaymentRequest, Result<SubtractPaymentResponse>> handler,
                CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(new SubtractPaymentRequest
            {
                ReservationId = reservationId,
                Body = request
            }, ct);

            return result.ToTypedResult();
        })
        .RequireAuthorization("AdminOnly")
        .WithTags("Reservations")
        .ProducesValidationProblem()
        .Produces<SubtractPaymentResponse>()
        .Produces(StatusCodes.Status404NotFound);
    }
}
