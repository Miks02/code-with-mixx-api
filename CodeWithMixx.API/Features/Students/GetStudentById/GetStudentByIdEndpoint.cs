using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Common.Results;
using Microsoft.AspNetCore.Mvc;

namespace CodeWithMixx.API.Features.Students.GetStudentById;

public class GetStudentByIdEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("students/{id}", async (
                [AsParameters] GetStudentByIdRequest request,
                IHandler<GetStudentByIdRequest, Result<GetStudentByIdResponse>> getStudentByIdHandler,
                CancellationToken ct) =>
        {
            var result = await getStudentByIdHandler.HandleAsync(request, ct);
            return result.ToTypedResult();
        })
        .RequireAuthorization("AdminOnly")
        .WithTags("Students")
        .Produces<GetStudentByIdResponse>()
        .Produces(StatusCodes.Status404NotFound);
    }
}
