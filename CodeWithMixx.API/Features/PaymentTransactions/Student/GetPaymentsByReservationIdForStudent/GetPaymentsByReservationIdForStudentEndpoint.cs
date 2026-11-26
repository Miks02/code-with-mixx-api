using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Common.Results;

namespace CodeWithMixx.API.Features.PaymentTransactions.Student.GetPaymentsByReservationIdForStudent;

public class GetPaymentsByReservationIdForStudentEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/student/reservations/{reservationId:int}/payments", async (
                IHandler<GetPaymentsByReservationIdForStudentRequest, Result<GetPaymentsByReservationIdForStudentResponse>> handler,
                CancellationToken ct,
                int reservationId) =>
            {
                var result = await handler.HandleAsync(new GetPaymentsByReservationIdForStudentRequest(reservationId), ct);
                return result.ToTypedResult();
            })
            .RequireAuthorization("StudentOnly")
            .WithTags("Payments")
            .Produces<GetPaymentsByReservationIdForStudentResponse>()
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound);
    }
}
