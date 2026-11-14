using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Domain.Entities.Inquiries;
using CodeWithMixx.API.Infrastructure.Exceptions;
using Discord;
using Discord.WebSocket;

namespace CodeWithMixx.API.Infrastructure.Discord;

public class DiscordInquirySender(
    DiscordSocketClient client,
    IConfiguration config) : IInquirySender
{
    private readonly ulong _channelId = config.GetValue<ulong>("Discord:ReservationsChannelId");
    
    public async Task SendInquiryAsync(Inquiry inquiry)
    {
        var channel = await client.GetChannelAsync(_channelId) as IMessageChannel;

        if (channel is null)
            throw new DiscordChannelNotFoundException(_channelId);
        
        var embed = new EmbedBuilder()
            .WithTitle("Stigao je novi upit!")
            .WithColor(Color.Green)
            .AddField("Poslao: ", $"{inquiry.FirstName} {inquiry.LastName}")
            .AddField("Email: ", inquiry.Email)
            .AddField("Broj telefona: ", inquiry.PhoneNumber)
            .AddField("Tip upita: ", inquiry.MapInquiryTypeToLocalizedName())
            .AddField("Poruka: ", inquiry.Message)
            .WithCurrentTimestamp()
            .Build();
      
        await channel.SendMessageAsync(embed: embed);
        
    }
    
}
