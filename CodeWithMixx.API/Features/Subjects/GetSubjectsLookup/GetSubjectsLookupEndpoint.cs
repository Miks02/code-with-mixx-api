using CodeWithMixx.API.Common.Interfaces;

namespace CodeWithMixx.API.Features.Subjects.GetSubjectsLookup;

public class GetSubjectsLookupEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/subjects/lookup",
                async (IHandler<GetSubjectsLookupRequest, IReadOnlyList<GetSubjectsLookupResponse>> getSubjectsLookupHandler) 
                    => await getSubjectsLookupHandler.HandleAsync(new GetSubjectsLookupRequest()))
            .WithTags("Subjects")
            .Produces<IReadOnlyList<GetSubjectsLookupResponse>>();
    }
}