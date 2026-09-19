using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Common.Results;
using CodeWithMixx.API.Domain.Entities.Reservations;
using CodeWithMixx.API.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CodeWithMixx.API.Features.Reservations.RestoreClassReservation;

public class RestoreClassReservationHandler(AppDbContext context) : IHandler<RestoreClassReservationRequest, Result>
{
    public async Task<Result> HandleAsync(RestoreClassReservationRequest request, CancellationToken ct = default)
    {
        var reservation = await context.Reservations
            .IgnoreQueryFilters()
            .Include(r => r.Classes)
            .FirstOrDefaultAsync(r => r.Id == request.ReservationId, ct);

        if (reservation is null)
            return Result.Failure(ReservationError.NotFound(request.ReservationId));

        var restoreResult = reservation.RestoreClassReservation();
        if (!restoreResult.IsSuccess)
            return restoreResult;

        await context.SaveChangesAsync(ct);

        return Result.Success();
    }
}
