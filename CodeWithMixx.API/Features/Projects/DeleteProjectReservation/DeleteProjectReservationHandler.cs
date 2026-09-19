using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Common.Results;
using CodeWithMixx.API.Domain.Entities.Reservations;
using CodeWithMixx.API.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CodeWithMixx.API.Features.Projects.DeleteProjectReservation;

public class DeleteProjectReservationHandler(AppDbContext context) : IHandler<DeleteProjectReservationRequest, Result>
{
    public async Task<Result> HandleAsync(DeleteProjectReservationRequest request, CancellationToken ct = default)
    {
        var reservation = await context.Reservations
            .Include(r => r.Projects)
            .FirstOrDefaultAsync(r => r.Id == request.ReservationId, ct);

        if (reservation is null)
            return Result.Failure(ReservationError.NotFound(request.ReservationId));

        if (reservation.RequiresHistoryRetention())
        {
            var deleteResult = reservation.DeleteProjectReservation();
            if (!deleteResult.IsSuccess)
                return deleteResult;
        }
        else
            context.Reservations.Remove(reservation);
        
        await context.SaveChangesAsync(ct);
        return Result.Success();
    }
}
