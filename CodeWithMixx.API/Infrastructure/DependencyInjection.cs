using CodeWithMixx.API.Infrastructure.Persistence;
using CodeWithMixx.API.Infrastructure.Security;

namespace CodeWithMixx.API.Infrastructure;

public static class DependencyInjection
{
    public static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")!;
        
        services.AddPersistence(connectionString);
        services.AddSecurity(configuration);

    }
    
    public static async Task MapSeeders(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
        
        await seeder.SeedRolesAsync();
        await seeder.SeedAdminAsync();
        Console.WriteLine("Rar");
    }
}