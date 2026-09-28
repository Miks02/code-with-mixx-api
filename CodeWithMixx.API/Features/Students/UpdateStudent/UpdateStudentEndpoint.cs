using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Common.Results;

namespace CodeWithMixx.API.Features.Students.UpdateStudent;

public class UpdateStudentEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("/admin/students/{id}", async (
                string id,
                UpdateStudentRequest request,
                IHandler<UpdateStudentRequest, Result<UpdateStudentResponse>> handler,
                CancellationToken ct) =>
            {
                var result = await handler.HandleAsync(request with {Id = id}, ct);
                return result.ToTypedResult();
            })
            .WithTags("Students")
            .RequireAuthorization("AdminOnly")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);
    }
}