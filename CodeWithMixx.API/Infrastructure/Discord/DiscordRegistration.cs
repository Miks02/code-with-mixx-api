using CodeWithMixx.API.Common.Interfaces;
using Discord;
using Discord.WebSocket;

namespace CodeWithMixx.API.Infrastructure.Discord;

public static class DiscordRegistration
{
    public static void AddDiscord(this IServiceCollection services)
    {
        var discordConfig = new DiscordSocketConfig
        {
            GatewayIntents = GatewayIntents.Guilds
        };
        services.AddSingleton(new DiscordSocketClient(discordConfig));
        services.AddHostedService<DiscordBotService>();
        services.AddScoped<IInquirySender, DiscordInquirySender>();
    }
}
