namespace CodeWithMixx.API.Infrastructure.Cors;

public static class CorsRegistration
{
    public static void RegisterCors(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("DevCors", policyBuilder =>
            {
                policyBuilder
                    .WithOrigins("https://localhost:4200")
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });
    }
}