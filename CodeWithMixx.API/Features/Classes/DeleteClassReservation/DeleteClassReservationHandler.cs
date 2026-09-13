using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Common.Results;
using CodeWithMixx.API.Domain.Entities.Reservations;
using CodeWithMixx.API.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CodeWithMixx.API.Features.Classes.DeleteClassReservation;

public class DeleteClassReservationHandler(AppDbContext context) : IHandler<DeleteClassReservationRequest, Result>
{
    public async Task<Result> HandleAsync(DeleteClassReservationRequest request, CancellationToken ct = default)
    {
        var reservation = await context.Reservations
            .Include(r => r.Classes)
            .FirstOrDefaultAsync(r => r.Id == request.ReservationId, ct);

        if (reservation is null)
            return Result.Failure(ReservationError.NotFound(request.ReservationId));

        if (reservation.RequiresHistoryRetention())
            reservation.Delete();
        else
            context.Reservations.Remove(reservation);

        await context.SaveChangesAsync(ct);

        return Result.Success();
    }
}
