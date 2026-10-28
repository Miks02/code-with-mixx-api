using CodeWithMixx.API.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CodeWithMixx.API.Infrastructure.BackgroundServices;

public class CleanExpiredTokensJob(
    ILogger<CleanExpiredTokensJob> logger,
    IServiceScopeFactory serviceScopeFactory,
    IConfiguration configuration) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var retentionInDays = configuration.GetValue<int>("RefreshConfig:RetentionThresholdInDays");
        var cleaningInterval = configuration.GetValue<int>("RefreshConfig:CleanupIntervalInDays");
        
        using var timer = new PeriodicTimer(TimeSpan.FromDays(cleaningInterval));
        do
        {
            try
            {
                logger.LogInformation("Starting a job for clearing expired refresh tokens...");
                var dateDelta = DateTime.UtcNow.AddDays(-retentionInDays);
                using var scope = serviceScopeFactory.CreateScope();

                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var affectedRows = await dbContext.RefreshTokens
                    .Where(rt => rt.ExpiresAt < dateDelta)
                    .ExecuteDeleteAsync(stoppingToken);
                
                if(affectedRows > 0)
                    logger.LogInformation("{AffectedRows} expired refresh tokens were deleted", affectedRows);

            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                logger.LogInformation("A job for clearing expired refresh tokens was cancelled during application shutdown");
            }
            catch (DbUpdateException ex)
            {
                logger.LogError(ex, "A database error occurred while cleaning expired tokens");
            }
            catch(Exception ex)
            {
                logger.LogError(ex, "An error occurred while cleaning expired tokens");
            }
        } while(await timer.WaitForNextTickAsync(stoppingToken));
    }
}