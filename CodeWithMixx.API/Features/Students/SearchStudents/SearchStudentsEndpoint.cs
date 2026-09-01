using CodeWithMixx.API.Common.Interfaces;

namespace CodeWithMixx.API.Features.Students.SearchStudents
{
    public class SearchStudentsEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet("/students/search", 
                async (
                    [AsParameters] SearchStudentsRequest request, 
                    IHandler<SearchStudentsRequest,
                    SearchStudentsResponse> handler,
                    CancellationToken ct) => await handler.HandleAsync(request, ct))
                .RequireAuthorization("AdminOnly")
                .WithTags("Students")
                .Produces<SearchStudentsResponse>();
        }
    }
}
