using System.Net;
using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Common.Results;

namespace CodeWithMixx.API.Features.Students.DeleteStudent
{
    public class DeleteStudentEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapDelete("/students/{id}", async ([AsParameters] DeleteStudentRequest request, IHandler<DeleteStudentRequest, Result> handler) =>
            {
                var result = await handler.HandleAsync(request);
                return result.ToTypedResult(HttpStatusCode.NoContent);
            })
            .RequireAuthorization("AdminOnly")
            .WithTags("Students")
            .Produces(StatusCodes.Status204NoContent);
        }
    }
}
