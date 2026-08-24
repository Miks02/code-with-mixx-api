using CodeWithMixx.API.Common.Markers;
using CodeWithMixx.API.Infrastructure.Persistence;
using CodeWithMixx.API.Infrastructure.Security;
using FluentValidation;

namespace CodeWithMixx.API.Infrastructure;

public static class DependencyInjection
{
    public static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")!;

        services.AddHttpContextAccessor();
        services.AddPersistence(connectionString);
        services.AddSecurity(configuration);
        services.AddHandlers();
        services.AddValidatorsFromAssembly(typeof(Program).Assembly);
        services.AddProblemDetails();
        services.AddOpenApi();
    }

    public static void AddHandlers(this IServiceCollection services)
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