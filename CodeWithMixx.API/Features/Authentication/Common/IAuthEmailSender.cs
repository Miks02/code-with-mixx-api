namespace CodeWithMixx.API.Features.Authentication.Common;

public interface IAuthEmailSender
{
    Task SendPasswordResetEmailAsync(string email, string token);
}