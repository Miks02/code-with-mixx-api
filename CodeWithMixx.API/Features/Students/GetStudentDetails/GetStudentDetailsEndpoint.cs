using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Common.Results;
using Microsoft.AspNetCore.Mvc;

namespace CodeWithMixx.API.Features.Students.GetStudentDetails;

public class GetStudentDetailsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("admin/students/{id}", async (
                [AsParameters] GetStudentDetailsRequest request,
                IHandler<GetStudentDetailsRequest, Result<GetStudentDetailsResponse>> handler,
                CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(request, ct);
            return result.ToTypedResult();
        })
        .RequireAuthorization("AdminOnly")
        .WithTags("Students")
        .Produces<GetStudentDetailsResponse>()
        .Produces(StatusCodes.Status404NotFound);
    }
}
