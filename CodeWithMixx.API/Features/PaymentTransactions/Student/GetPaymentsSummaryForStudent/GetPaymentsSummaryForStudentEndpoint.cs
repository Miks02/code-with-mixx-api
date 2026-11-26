using CodeWithMixx.API.Common.Interfaces;

namespace CodeWithMixx.API.Features.PaymentTransactions.Student.GetPaymentsSummaryForStudent;

public class GetPaymentsSummaryForStudentEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/student/payments/summary",
            async (
                [AsParameters] GetPaymentsSummaryForStudentRequest request,
                IHandler<GetPaymentsSummaryForStudentRequest, GetPaymentsSummaryForStudentResponse> handler,
                CancellationToken ct) =>
            {
                var result = await handler.HandleAsync(request, ct);
                return result;
            })
            .RequireAuthorization("StudentOnly")
            .WithTags("Payments")
            .Produces<GetPaymentsSummaryForStudentResponse>()
            .Produces(StatusCodes.Status401Unauthorized);
    }
}
