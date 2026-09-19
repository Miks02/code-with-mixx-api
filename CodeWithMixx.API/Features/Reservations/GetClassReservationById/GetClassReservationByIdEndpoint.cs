using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Common.Results;
using Microsoft.AspNetCore.Mvc;

namespace CodeWithMixx.API.Features.Reservations.GetClassReservationById;

public class GetClassReservationByIdEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/admin/classes/{reservationId:int}", async (
                [AsParameters] GetClassReservationByIdRequest request,
                IHandler<GetClassReservationByIdRequest, Result<GetClassReservationByIdResponse>> handler,
                CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(request, ct);
            return result.ToTypedResult();
        })
        .RequireAuthorization("AdminOnly")
        .WithTags("Classes")
        .Produces<GetClassReservationByIdResponse>()
        .Produces(StatusCodes.Status404NotFound);
    }
}
