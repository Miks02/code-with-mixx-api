using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Common.Results;

namespace CodeWithMixx.API.Features.PaymentTransactions.Admin.GetPaymentsByReservationIdForAdmin;

public class GetPaymentsByReservationIdForAdminEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/reservations/{reservationId:int}/payments", async (
                IHandler<GetPaymentsByReservationIdForAdminRequest, Result<GetPaymentsByReservationIdForAdminResponse>> handler,
                CancellationToken ct,
                int reservationId) =>
            {
                var result = await handler.HandleAsync(new GetPaymentsByReservationIdForAdminRequest(reservationId), ct);
                return result.ToTypedResult();
            })
            .RequireAuthorization()
            .WithTags("Payments")
            .Produces<GetPaymentsByReservationIdForAdminResponse>()
            .Produces(StatusCodes.Status404NotFound);

    }
}