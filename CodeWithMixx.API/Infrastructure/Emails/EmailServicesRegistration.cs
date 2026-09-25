using CodeWithMixx.API.Common.Interfaces;
using Resend;

namespace CodeWithMixx.API.Infrastructure.Emails;

public static class EmailServicesRegistration
{
    public static void AddEmailServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpClient<IResend, ResendClient>();
        services.Configure<ResendClientOptions>(options =>
        {
            options.ApiToken = configuration["Resend:ApiToken"] ??
                               throw new InvalidOperationException("Resend API token is not configured.");
        });
        services.AddTransient<IAuthEmailSender, AuthEmailSender>();
    }
}