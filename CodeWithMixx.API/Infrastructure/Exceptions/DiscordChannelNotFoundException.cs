namespace CodeWithMixx.API.Infrastructure.Exceptions;

public class DiscordChannelNotFoundException : Exception
{
    public DiscordChannelNotFoundException(ulong channelId) : base($"Discord channel with id {channelId} has not been found.")
    {
    }
}