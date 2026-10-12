using CodeWithMixx.API.Common.Interfaces;

namespace CodeWithMixx.API.Features.Students.GetStudentsLookup;

public class GetStudentsLookupEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/admin/students/lookup",
                async (IHandler<GetStudentsLookupRequest, IReadOnlyList<GetStudentsLookupResponse>> getStudentsLookupHandler, CancellationToken ct)
                    => await getStudentsLookupHandler.HandleAsync(new GetStudentsLookupRequest(), ct))
            .WithTags("Students")
            .RequireAuthorization("AdminOnly")
            .Produces<IReadOnlyList<GetStudentsLookupResponse>>();
    }
}
