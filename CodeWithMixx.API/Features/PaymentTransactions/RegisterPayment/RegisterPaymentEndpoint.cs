using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Common.Results;

namespace CodeWithMixx.API.Features.PaymentTransactions.RegisterPayment;

public class RegisterPaymentEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/admin/reservations/{reservationId:int}/payments/register", async (
                int reservationId,
                RegisterPaymentBody request,
                IHandler<RegisterPaymentRequest, Result<RegisterPaymentResponse>> handler,
                CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(new RegisterPaymentRequest
            {
                ReservationId = reservationId,
                Body = request
            }, ct);

            return result.ToTypedResult();
        })
        .RequireAuthorization("AdminOnly")
        .WithTags("Payments")
        .ProducesValidationProblem()
        .Produces<RegisterPaymentResponse>()
        .Produces(StatusCodes.Status404NotFound);
    }
}
