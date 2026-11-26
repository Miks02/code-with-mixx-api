using CodeWithMixx.API.Common.Interfaces;

namespace CodeWithMixx.API.Features.Reservations.Admin.GetOpenBalanceForAdmin;

public class GetOpenBalanceForAdminEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/admin/reservations/open-balance", async (
                IHandler<GetOpenBalanceForAdminRequest, GetOpenBalanceForAdminResponse> handler,
                CancellationToken ct) =>
        {
            var response = await handler.HandleAsync(new GetOpenBalanceForAdminRequest(), ct);
            return Results.Ok(response);
        })
        .RequireAuthorization("AdminOnly")
        .WithTags("Reservations")
        .Produces<GetOpenBalanceForAdminResponse>()
        .Produces(StatusCodes.Status401Unauthorized);
    }
}
