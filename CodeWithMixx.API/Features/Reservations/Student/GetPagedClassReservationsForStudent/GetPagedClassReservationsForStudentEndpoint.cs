using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Common.Results;

namespace CodeWithMixx.API.Features.Reservations.Student.GetPagedClassReservationsForStudent;

public class GetPagedClassReservationsForStudentEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/student/reservations/classes", async (
                [AsParameters] GetPagedClassReservationsForStudentRequest request,
                IHandler<GetPagedClassReservationsForStudentRequest, Result<PagedResult<GetPagedClassReservationsForStudentResponse>>> handler,
                CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(request, ct);
            return result.ToTypedResult();
        })
        .RequireAuthorization("StudentOnly")
        .WithTags("Reservations")
        .Produces<PagedResult<GetPagedClassReservationsForStudentResponse>>()
        .Produces(StatusCodes.Status401Unauthorized);
    }
}
