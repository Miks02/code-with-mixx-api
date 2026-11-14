using Discord;
using Discord.WebSocket;

namespace CodeWithMixx.API.Infrastructure.BackgroundServices;

public class DiscordBotService(
    ILogger<DiscordBotService> logger,
    IConfiguration configuration,
    DiscordSocketClient client) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        client.Log += LogAsync;

        client.Ready += () =>
        {
            logger.LogInformation("CodeWithMixx Bot is online!");
            return Task.CompletedTask;
        };

        await client.LoginAsync(TokenType.Bot, configuration["Discord:BotToken"]);
        await client.StartAsync();

        await Task.Delay(-1, stoppingToken);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await client.StopAsync();
        await client.LogoutAsync();
        await base.StopAsync(cancellationToken);
    }

    private Task LogAsync(LogMessage log)
    {
        logger.LogInformation("[Discord]: {LogMessage}", log.Message);
        return Task.CompletedTask;
    }
}
