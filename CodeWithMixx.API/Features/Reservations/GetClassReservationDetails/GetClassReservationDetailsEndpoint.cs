using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Common.Results;
using Microsoft.AspNetCore.Mvc;

namespace CodeWithMixx.API.Features.Reservations.GetClassReservationDetails;

public class GetClassReservationDetailsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/admin/reservations/classes/{reservationId:int}", async (
                [AsParameters] GetClassReservationDetailsRequest request,
                IHandler<GetClassReservationDetailsRequest, Result<GetClassReservationDetailsResponse>> handler,
                CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(request, ct);
            return result.ToTypedResult();
        })
        .RequireAuthorization("AdminOnly")
        .WithTags("Reservations")
        .Produces<GetClassReservationDetailsResponse>()
        .Produces(StatusCodes.Status404NotFound);
    }
}
