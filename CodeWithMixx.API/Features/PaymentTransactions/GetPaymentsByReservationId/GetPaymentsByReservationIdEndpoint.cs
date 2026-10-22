using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Common.Results;

namespace CodeWithMixx.API.Features.PaymentTransactions.GetPaymentsByReservationId;

public class GetPaymentsByReservationIdEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/reservations/{reservationId:int}/payments", async (
                IHandler<GetPaymentsByReservationIdRequest, Result<GetPaymentsByReservationIdResponse>> handler,
                CancellationToken ct,
                int reservationId) =>
            {
                var result = await handler.HandleAsync(new GetPaymentsByReservationIdRequest(reservationId), ct);
                return result.ToTypedResult();
            })
            .RequireAuthorization()
            .WithTags("Payments")
            .Produces<GetPaymentsByReservationIdResponse>()
            .Produces(StatusCodes.Status404NotFound);

    }
}