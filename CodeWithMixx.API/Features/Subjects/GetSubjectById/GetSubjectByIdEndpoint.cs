using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Common.Results;
using Microsoft.AspNetCore.Mvc;

namespace CodeWithMixx.API.Features.Subjects.GetSubjectById;

public class GetSubjectByIdEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("subjects/{id:int}", async (
                [AsParameters] GetSubjectByIdRequest request, 
                IHandler<GetSubjectByIdRequest, 
                Result<GetSubjectByIdResponse>> getSubjectByIdHandler, CancellationToken ct) =>
        {
            var result = await getSubjectByIdHandler.HandleAsync(request, ct);
            return result.ToTypedResult();
        })
        .RequireAuthorization()
        .WithTags("Subjects")
        .Produces<GetSubjectByIdResponse>()
        .Produces(StatusCodes.Status404NotFound);
    }
}