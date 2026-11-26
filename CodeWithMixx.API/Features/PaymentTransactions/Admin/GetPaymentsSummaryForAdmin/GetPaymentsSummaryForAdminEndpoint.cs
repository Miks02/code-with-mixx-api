using CodeWithMixx.API.Common.Interfaces;

namespace CodeWithMixx.API.Features.PaymentTransactions.Admin.GetPaymentsSummaryForAdmin;

public class GetPaymentsSummaryForAdminEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/admin/payments/summary",
            async (
                [AsParameters] GetPaymentsSummaryForAdminRequest request,
                IHandler<GetPaymentsSummaryForAdminRequest, GetPaymentsSummaryForAdminResponse> handler,
                CancellationToken ct) =>
            {
                var result = await handler.HandleAsync(request, ct);
                return result;
            })
            .RequireAuthorization("AdminOnly")
            .WithTags("Payments")
            .Produces<GetPaymentsSummaryForAdminResponse>()
            .Produces(StatusCodes.Status401Unauthorized);
    }
}
