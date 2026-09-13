using System.Net;
using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Common.Results;
using Microsoft.AspNetCore.Mvc;

namespace CodeWithMixx.API.Features.Classes.CreateClassReservation;

public class CreateClassReservationEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/classes", async (
                CreateClassReservationRequest request,
                IHandler<CreateClassReservationRequest, Result<CreateClassReservationResponse>> handler,
                CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(request, ct);
            return result.ToTypedResult(HttpStatusCode.Created, $"/api/classes/{result.Payload?.Id}");
        })
        .RequireAuthorization("AdminOnly")
        .WithTags("Classes")
        .ProducesValidationProblem()
        .Produces<Result<CreateClassReservationResponse>>();
    }
}