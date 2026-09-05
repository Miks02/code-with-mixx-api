using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Infrastructure.Cors;
using CodeWithMixx.API.Infrastructure.Decorators;
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
        services.AddExceptionHandler<TokensRevokedExceptionHandler>();
        services.AddExceptionHandler<SecurityDbUpdateExceptionHandler>();
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddOpenApi();
        services.RegisterCors();
        
        services.Scan(scan => scan.FromAssemblyOf<Program>()
            .AddClasses(classes => classes.AssignableTo(typeof(IHandler<,>)))
            .AsImplementedInterfaces()
            .WithScopedLifetime());
        
        services.Decorate(typeof(IHandler<,>), typeof(LoggingDecorator<,>));
    }
    
    public static async Task MapSeeders(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
        
        await seeder.SeedRolesAsync();
        await seeder.SeedAdminAsync();
    }
}