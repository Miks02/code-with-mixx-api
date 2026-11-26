using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Common.Results;
using CodeWithMixx.API.Features.PaymentTransactions.Common;

namespace CodeWithMixx.API.Features.PaymentTransactions.Student.GetPagedPaymentsForStudent;

public class GetPagedPaymentsForStudentEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/student/payments",
            async (
                [AsParameters] GetPagedPaymentsForStudentRequest request,
                IHandler<GetPagedPaymentsForStudentRequest, PagedResult<PaymentTransactionItem>> handler,
                CancellationToken ct) =>
            {
                var result = await handler.HandleAsync(request, ct);
                return result;
            })
            .RequireAuthorization("StudentOnly")
            .WithTags("Payments")
            .Produces<PagedResult<PaymentTransactionItem>>()
            .Produces(StatusCodes.Status401Unauthorized);
    }
}
