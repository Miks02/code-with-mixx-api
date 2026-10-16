using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Common.Results;
using Microsoft.AspNetCore.Mvc;

namespace CodeWithMixx.API.Features.Reservations.GetProjectReservationDetails;

public class GetProjectReservationDetailsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/admin/reservations/projects/{reservationId:int}", async (
                [AsParameters] GetProjectReservationDetailsRequest request,
                IHandler<GetProjectReservationDetailsRequest, Result<GetProjectReservationDetailsResponse>> handler,
                CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(request, ct);
            return result.ToTypedResult();
        })
        .RequireAuthorization("AdminOnly")
        .WithTags("Reservations")
        .Produces<GetProjectReservationDetailsResponse>()
        .Produces(StatusCodes.Status404NotFound);
    }
}
