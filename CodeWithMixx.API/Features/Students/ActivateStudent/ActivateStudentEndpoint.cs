using System.Net;
using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Common.Results;

namespace CodeWithMixx.API.Features.Students.ActivateStudent
{
    public class ActivateStudentEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("/admin/students/{id:guid}/activate", async (
                    [AsParameters] ActivateStudentRequest request,
                    IHandler<ActivateStudentRequest, Result> handler,
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
