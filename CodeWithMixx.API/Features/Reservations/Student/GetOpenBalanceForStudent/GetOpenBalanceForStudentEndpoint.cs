using CodeWithMixx.API.Common.Interfaces;

namespace CodeWithMixx.API.Features.Reservations.Student.GetOpenBalanceForStudent;

public class GetOpenBalanceForStudentEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/student/reservations/open-balance", async (
                IHandler<GetOpenBalanceForStudentRequest, GetOpenBalanceForStudentResponse> handler,
                CancellationToken ct) =>
        {
            var response = await handler.HandleAsync(new GetOpenBalanceForStudentRequest(), ct);
            return Results.Ok(response);
        })
        .RequireAuthorization("StudentOnly")
        .WithTags("Reservations")
        .Produces<GetOpenBalanceForStudentResponse>()
        .Produces(StatusCodes.Status401Unauthorized);
    }
}
