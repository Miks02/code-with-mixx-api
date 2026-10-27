using CodeWithMixx.API.Domain.Entities.Reservations;
using CodeWithMixx.API.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CodeWithMixx.API.Infrastructure.BackgroundServices;

public class MarkOverdueReservationsJob(
    ILogger<MarkOverdueReservationsJob> logger,
    IServiceScopeFactory serviceScopeFactory,
    IConfiguration configuration) : BackgroundService
{
    
    private readonly int _intervalInDays = configuration.GetValue<int>("BackgroundServices:MarkOverdueReservationsJob:IntervalInDays");
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromDays(_intervalInDays));
        
        do
        {
            try
            {
                logger.LogInformation("Starting the job for overdue reservations...");
                using var scope = serviceScopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                var now = DateTime.UtcNow;
                var today = DateOnly.FromDateTime(now);

                var projectReservationsWithOverduePayment = await context.Reservations
                    .Where(r => r.PaidAmount < r.TotalPrice &&
                                r.ReservationStatus != ReservationStatus.Cancelled &&
                                r.PaymentStatus != PaymentStatus.Overdue &&
                                r.ReservationType == ReservationType.Project &&
                                r.Projects.Max(p => p.EndDate) < today)
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(rp => rp.PaymentStatus, PaymentStatus.Overdue)
                        .SetProperty(rp => rp.UpdatedAt, now), stoppingToken);

                if (projectReservationsWithOverduePayment > 0)
                    logger.LogInformation("Updated payment status for {Count} project based reservations",
                        projectReservationsWithOverduePayment);

                var classReservationsWithOverduePayment = await context.Reservations
                    .Where(r => r.PaidAmount < r.TotalPrice &&
                                r.ReservationStatus != ReservationStatus.Cancelled &&
                                r.PaymentStatus != PaymentStatus.Overdue &&
                                r.ReservationType == ReservationType.Class &&
                                r.Classes.Max(c => c.EndsAt) < now)
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(rp => rp.PaymentStatus, PaymentStatus.Overdue)
                        .SetProperty(rp => rp.UpdatedAt, now), stoppingToken);

                if (classReservationsWithOverduePayment > 0)
                    logger.LogInformation("Updated payment status for {Count} class based reservations",
                        classReservationsWithOverduePayment);

                logger.LogInformation("A job for overdue reservations has finished");
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                logger.LogInformation("A job for overdue reservations was cancelled during application shutdown");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while trying to update payment status in reservations");
            }
        } while(await timer.WaitForNextTickAsync(stoppingToken));
    }
}