using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Common.Results;
using Microsoft.AspNetCore.Mvc;

namespace CodeWithMixx.API.Features.Reservations.CreateProjectReservation;

public class CreateProjectReservationEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/admin/projects", async 
            (
                CreateProjectReservationRequest request,
                IHandler<CreateProjectReservationRequest, Result<CreateProjectReservationResponse>> handler,
                CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(request, ct);
            return result.ToTypedResult();
        })
        .WithTags("Projects")
        .RequireAuthorization("AdminOnly")
        .Produces<CreateProjectReservationResponse>()
        .Produces<ValidationProblemDetails>(StatusCodes.Status400BadRequest);
    }
}