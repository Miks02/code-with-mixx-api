using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Common.Results;
using CodeWithMixx.API.Domain.Entities.Reservations;
using CodeWithMixx.API.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CodeWithMixx.API.Features.Projects.RestoreProjectReservation;

public class RestoreProjectReservationHandler(AppDbContext context) : IHandler<RestoreProjectReservationRequest, Result>
{
    public async Task<Result> HandleAsync(RestoreProjectReservationRequest request, CancellationToken ct = default)
    {
        var reservation = await context.Reservations
            .IgnoreQueryFilters()
            .Include(r => r.Projects)
            .FirstOrDefaultAsync(r => r.Id == request.ReservationId, ct);

        if (reservation is null)
            return Result.Failure(ReservationError.NotFound(request.ReservationId));

        var restoreResult = reservation.RestoreProjectReservation();
        if (!restoreResult.IsSuccess)
            return restoreResult;

        await context.SaveChangesAsync(ct);

        return Result.Success();
    }
}
