using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Infrastructure.BackgroundServices;
using CodeWithMixx.API.Infrastructure.Cors;
using CodeWithMixx.API.Infrastructure.Decorators;
using CodeWithMixx.API.Infrastructure.Emails;
using CodeWithMixx.API.Infrastructure.Exceptions.Handlers;
using CodeWithMixx.API.Infrastructure.Persistence;
using CodeWithMixx.API.Infrastructure.RateLimiting;
using CodeWithMixx.API.Infrastructure.Security;
using CodeWithMixx.API.Infrastructure.Utility;
using FluentValidation;

namespace CodeWithMixx.API.Infrastructure;

public static class DependencyInjection
{
    public static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")!;
        services.AddHttpContextAccessor();
        services.AddUtility();
        services.AddGlobalRateLimiter();
        services.AddAuthRateLimiter();
        services.AddForgotPasswordRateLimiter();
        services.AddInvitationRateLimiter();
        services.AddPersistence(connectionString);
        services.AddSecurity(configuration);
        services.AddEmailServices(configuration);
        services.AddValidatorsFromAssembly(typeof(Program).Assembly);
        services.AddExceptionHandler<TokensRevokedExceptionHandler>();
        services.AddExceptionHandler<SecurityDbUpdateExceptionHandler>();
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddHostedService<CleanExpiredTokensJob>();
        services.AddHostedService<MarkOverdueReservationsJob>();
        services.AddOpenApi();
        services.RegisterCors();
        services.AddCustomProblemDetails();
        
        
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

    private static void AddCustomProblemDetails(this IServiceCollection services)
    {
        services.AddProblemDetails(options =>
        {
            options.CustomizeProblemDetails = ctx =>
            {
                ctx.ProblemDetails.Extensions.TryAdd("errorCode", ctx.ProblemDetails.Status switch
                {
                    400 => "General.BadRequest",
                    401 => "General.Unauthorized",
                    403 => "General.Forbidden",
                    404 => "General.NotFound",
                    409 => "General.Conflict",
                    422 => "General.UnprocessableEntity",
                    429 => "General.TooManyRequests",
                    _ => "General.InternalServerError",
                });
                
                ctx.ProblemDetails.Title = ctx.ProblemDetails.Status switch
                {
                    400 => "Bad request",
                    401 => "Unauthorized",
                    403 => "Forbidden",
                    404 => "Not found",
                    409 => "Conflict",
                    429 => "Too many requests",
                    _ => "Internal server error"
                };

                ctx.ProblemDetails.Instance = ctx.HttpContext.Request.Path;
                ctx.ProblemDetails.Type = $"urn:codewithmixx-api:error:{ctx.ProblemDetails.Extensions["errorCode"]}";
            };
        });
    }
}