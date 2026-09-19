using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Common.Results;
using Microsoft.AspNetCore.Mvc;

namespace CodeWithMixx.API.Features.Reservations.GetPagedClassReservations;

public class GetPagedClassReservationsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/admin/reservations/classes", async (
                [AsParameters] GetPagedClassReservationsRequest request,
                IHandler<GetPagedClassReservationsRequest, Result<PagedResult<GetPagedClassReservationsResponse>>> handler,
                CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(request, ct);
            return result.ToTypedResult();
        })
        .RequireAuthorization("AdminOnly")
        .WithTags("Reservations")
        .Produces<PagedResult<GetPagedClassReservationsResponse>>();
    }
}
