using CodeWithMixx.API.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace CodeWithMixx.API.Infrastructure.Persistence.Interceptors;

public class AuditInterceptor(TimeProvider timeProvider) : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        var context = eventData.Context;

        if (context is null) return base.SavingChangesAsync(eventData, result, cancellationToken);

        var currentTime = timeProvider.GetUtcNow().UtcDateTime;
        
        foreach (var entry in context.ChangeTracker.Entries<IAuditable>())
        {
            if (entry.State == EntityState.Added)
                entry.Property("CreatedAt").CurrentValue = currentTime;
            
            if (entry.State == EntityState.Modified)
                entry.Property("UpdatedAt").CurrentValue = currentTime;
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }
    
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        var context = eventData.Context;

        if (context is null) return base.SavingChanges(eventData, result);

        var currentTime = timeProvider.GetUtcNow().UtcDateTime;
        
        foreach (var entry in context.ChangeTracker.Entries<IAuditable>())
        {
            if (entry.State == EntityState.Added)
                entry.Property("CreatedAt").CurrentValue = currentTime;
            
            if (entry.State == EntityState.Modified)
                entry.Property("UpdatedAt").CurrentValue = currentTime;
        }

        return base.SavingChanges(eventData, result);
    }
}