using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Common.Results;
using Microsoft.AspNetCore.Mvc;

namespace CodeWithMixx.API.Features.Reservations.GetClassReservationsSummary;

public class GetClassReservationsSummaryEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/admin/reservations/classes/summary", async (
                [AsParameters] GetClassReservationsSummaryRequest request,
                IHandler<GetClassReservationsSummaryRequest, Result<GetClassReservationsSummaryResponse>> handler,
                CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(request, ct);
            return result.ToTypedResult();
        })
        .RequireAuthorization("AdminOnly")
        .WithTags("Reservations")
        .Produces<GetClassReservationsSummaryResponse>();
    }
}
