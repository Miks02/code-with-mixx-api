using System.Net;
using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Common.Results;

namespace CodeWithMixx.API.Features.Students.DeactivateStudent
{
    public class DeactivateStudentEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("/admin/students/{id:guid}/deactivate", async (
                    [AsParameters] DeactivateStudentRequest request,
                    IHandler<DeactivateStudentRequest, Result> handler,
                    CancellationToken ct) =>
            {
                var result = await handler.HandleAsync(request, ct);
                return result.ToTypedResult(HttpStatusCode.NoContent);
            })
            .RequireAuthorization("AdminOnly")
            .WithTags("Students")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status409Conflict);
        }
    }
}
