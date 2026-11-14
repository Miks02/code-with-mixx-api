using CodeWithMixx.API.Infrastructure.BackgroundServices;
using Discord;
using Discord.WebSocket;

namespace CodeWithMixx.API.Infrastructure.Discord;

public static class DiscordRegistration
{
    public static void AddDiscord(this IServiceCollection services)
    {
        var discordConfig = new DiscordSocketConfig
        {
            GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent
        };
        services.AddSingleton(new DiscordSocketClient(discordConfig));
        services.AddHostedService<DiscordBotService>();
    }
}
