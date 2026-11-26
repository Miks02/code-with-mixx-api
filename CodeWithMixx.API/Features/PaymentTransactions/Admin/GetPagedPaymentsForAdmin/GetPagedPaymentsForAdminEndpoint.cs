using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Common.Results;
using CodeWithMixx.API.Features.PaymentTransactions.Common;

namespace CodeWithMixx.API.Features.PaymentTransactions.Admin.GetPagedPaymentsForAdmin;

public class GetPagedPaymentsForAdminEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/admin/payments",
            async (
                [AsParameters] GetPagedPaymentsForAdminRequest request,
                IHandler<GetPagedPaymentsForAdminRequest, PagedResult<PaymentTransactionItem>> handler,
                CancellationToken ct) =>
            {
                var result = await handler.HandleAsync(request, ct);
                return result;
            })
            .RequireAuthorization("AdminOnly")
            .WithTags("Payments")
            .Produces<PagedResult<PaymentTransactionItem>>()
            .Produces(StatusCodes.Status401Unauthorized);
    }
}
