namespace CodeWithMixx.API.Common.Interfaces;

public interface IAuthEmailSender
{
    Task SendPasswordResetEmailAsync(string email, string userId, string token);
    Task SendInvitationEmailAsync(string email, string userId, string token);
}