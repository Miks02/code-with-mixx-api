using System.Net;
using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Common.Results;

namespace CodeWithMixx.API.Features.Students.CreateStudent
{
    public class CreateStudentEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("/students", async (CreateStudentRequest request, IHandler<CreateStudentRequest, Result<CreateStudentResponse>> handler, CancellationToken ct) =>
                {
                    var result = await handler.HandleAsync(request, ct);

                    return result.ToTypedResult(HttpStatusCode.Created, $"/api/students/{result.Payload?.Id}");
                })
            .WithTags("Students")
            .RequireAuthorization("AdminOnly")
            .Produces(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest);
        }
    }
}
