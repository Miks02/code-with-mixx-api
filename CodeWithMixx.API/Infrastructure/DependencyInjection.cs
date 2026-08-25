using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Infrastructure.Exceptions.Handlers;
using CodeWithMixx.API.Infrastructure.Persistence;
using CodeWithMixx.API.Infrastructure.RateLimiting;
using CodeWithMixx.API.Infrastructure.Security;
using FluentValidation;

namespace CodeWithMixx.API.Infrastructure;

public static class DependencyInjection
{
    public static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")!;
        services.AddHttpContextAccessor();
        services.AddGlobalRateLimiter();
        services.AddAuthRateLimiter();
        services.AddPersistence(connectionString);
        services.AddSecurity(configuration);
        services.AddValidatorsFromAssembly(typeof(Program).Assembly);
        services.AddProblemDetails();
        services.AddHandlers();
        services.AddExceptionHandler<TokensRevokedExceptionHandler>();
        services.AddExceptionHandler<SecurityDbUpdateExceptionHandler>();
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddOpenApi();
    }

    private static void AddHandlers(this IServiceCollection services)
    {
        var handlers = typeof(Program).Assembly
            .GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && typeof(IHandler).IsAssignableFrom(t));
        
        foreach (var handler in handlers)
        {
            services.AddScoped(handler);
        }
    }
    
    public static async Task MapSeeders(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
        
        await seeder.SeedRolesAsync();
        await seeder.SeedAdminAsync();
    }
}