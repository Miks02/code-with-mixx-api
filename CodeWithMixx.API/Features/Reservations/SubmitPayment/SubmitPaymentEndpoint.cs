using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Common.Results;

namespace CodeWithMixx.API.Features.Reservations.SubmitPayment;

public class SubmitPaymentEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/admin/reservations/{reservationId:int}/payments", async (
                int reservationId,
                SubmitPaymentBody request,
                IHandler<SubmitPaymentRequest, Result<SubmitPaymentResponse>> handler,
                CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(new SubmitPaymentRequest
            {
                ReservationId = reservationId,
                Body = request
            }, ct);
            
            return result.ToTypedResult();
        })
        .RequireAuthorization("AdminOnly")
        .WithTags("Reservations")
        .ProducesValidationProblem()
        .Produces<SubmitPaymentResponse>()
        .Produces(StatusCodes.Status404NotFound);
    }
}
