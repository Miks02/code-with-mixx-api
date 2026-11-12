using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Common.Results;

namespace CodeWithMixx.API.Features.Account.UpdateName;

public class UpdateNameEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("account/name", async (
                UpdateNameRequest request,
                IHandler<UpdateNameRequest, Result<UpdateNameResponse>> handler,
                CancellationToken ct) =>
            {
                var result = await handler.HandleAsync(request, ct);
                return result.ToTypedResult();
            })
            .RequireAuthorization()
            .WithTags("Account")
            .Produces<UpdateNameResponse>()
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound);
    }
}
