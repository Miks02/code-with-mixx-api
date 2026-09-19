using System.Net;
using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Common.Results;

namespace CodeWithMixx.API.Features.Classes.ArchiveClassReservation;

public class ArchiveClassReservationEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/admin/classes/{reservationId:int}/archive", async (
                [AsParameters] ArchiveClassReservationRequest request,
                IHandler<ArchiveClassReservationRequest, Result> handler,
                CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(request, ct);
            return result.ToTypedResult(HttpStatusCode.NoContent);
        })
        .RequireAuthorization("AdminOnly")
        .WithTags("Classes")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status409Conflict);
    }
}
