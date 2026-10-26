using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Common.Results;
using CodeWithMixx.API.Domain.Entities.Reservations;
using CodeWithMixx.API.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CodeWithMixx.API.Features.Reservations.DeleteReservation;

public class DeleteReservationHandler(AppDbContext context, IUserClock userClock) : IHandler<DeleteReservationRequest, Result>
{
    public async Task<Result> HandleAsync(DeleteReservationRequest request, CancellationToken ct = default)
    {
        var reservationToDelete = await context.Reservations
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(r => r.Id == request.ReservationId, ct);
        
        if(reservationToDelete is null)
            return Result.Failure(ReservationError.NotFound(request.ReservationId)); 
        
        if(reservationToDelete.RequiresHistoryRetention(userClock.Today))
            return Result.Failure(ReservationError.RequiresHistoryRetention(request.ReservationId));
        
        context.Reservations.Remove(reservationToDelete);
        await context.SaveChangesAsync(ct);
        
        return Result.Success();
    }
}