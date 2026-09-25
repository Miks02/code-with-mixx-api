namespace CodeWithMixx.API.Common.Interfaces;

public interface IAuthEmailSender
{
    Task SendPasswordResetEmailAsync(string email, string userId, string token);
}