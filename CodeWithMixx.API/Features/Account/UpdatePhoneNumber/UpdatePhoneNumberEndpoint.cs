using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Common.Results;

namespace CodeWithMixx.API.Features.Account.UpdatePhoneNumber;

public class UpdatePhoneNumberEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("/account/phone-number", async (
            UpdatePhoneNumberRequest request, 
            IHandler<UpdatePhoneNumberRequest, Result<UpdatePhoneNumberResponse>> handler,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(request, ct);
            return result.ToTypedResult();
        })
        .RequireAuthorization()
        .WithTags("Account")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status401Unauthorized);
    }
}