using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Common.Results;
using CodeWithMixx.API.Domain.Entities.Reservations;
using CodeWithMixx.API.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CodeWithMixx.API.Features.Reservations.ArchiveProjectReservation;

public class ArchiveProjectReservationHandler(AppDbContext context) : IHandler<ArchiveProjectReservationRequest, Result>
{
    public async Task<Result> HandleAsync(ArchiveProjectReservationRequest request, CancellationToken ct = default)
    {
        var reservation = await context.Reservations
            .IgnoreQueryFilters()
            .Include(r => r.Projects.Where(p => !p.IsDeleted))
            .FirstOrDefaultAsync(r => r.Id == request.ReservationId, ct);

        if (reservation is null)
            return Result.Failure(ReservationError.NotFound(request.ReservationId));

        var archiveResult = reservation.DeleteProjectReservation();
        if (!archiveResult.IsSuccess)
            return archiveResult;

        await context.SaveChangesAsync(ct);

        return Result.Success();
    }
}
