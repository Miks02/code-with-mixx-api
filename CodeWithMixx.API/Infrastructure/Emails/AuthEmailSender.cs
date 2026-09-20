using System.Text.Encodings.Web;
using CodeWithMixx.API.Features.Authentication.Common;
using Resend;

namespace CodeWithMixx.API.Infrastructure.Emails;

public class AuthEmailSender(IConfiguration configuration, IResend resend) : IAuthEmailSender
{
    private readonly string _clientUrl = configuration["Web:ClientUrl"]!;
    private readonly string _fromEmail = configuration["Resend:EmailSender"]!;
    
    public async Task SendPasswordResetEmailAsync(string email, string userId, string token)
    {
        token = UrlEncoder.Default.Encode(token);
        
        var resetUrl = $"{_clientUrl}/reset-password?token={token}&userId={userId}";
        
        var htmlBody = $"""
                         <!DOCTYPE html>
                         <html lang="sr">
                         <head>
                             <meta charset="UTF-8">
                             <meta name="viewport" content="width=device-width, initial-scale=1.0">
                         </head>
                         <body style="font-family: Arial, sans-serif; background-color: #d1fae5; color: #51545e; margin: 0; padding: 20px; font-weight: 600;">
                             <div style="max-width: 600px; background-color: #064e3b; margin: 0 auto; padding: 30px; border-radius: 8px; color: #ecfdf5;">
                                 <h2 style="color: #ecfdf5; margin-top: 0;">Resetovanje lozinke</h2>
                                 <p>Pozdrav,</p>
                                 <p>Primili smo zahtev za resetovanje lozinke za tvoj nalog.</p>
                                 <p>Klikni na dugme ispod kako bi postavio/la novu lozinku:</p>
                                 
                                 <div style="text-align: center; margin: 30px 0;">
                                     <a href="{resetUrl}" 
                                        style="background-color: #d97706; color: #ffffff; text-decoration: none; padding: 12px 24px; border-radius: 6px; font-weight: bold; display: inline-block; box-shadow: 0 10px 15px -3px rgba(0, 0, 0, 0.3), 0 4px 6px -4px rgba(0, 0, 0, 0.3);">
                                         Resetuj lozinku
                                     </a>
                                 </div>
                                 
                                 <p style="text-align: center; font-size: 13px; color: #a7f3d0; margin: 0 0 30px;">
                                    Link važi 1 sat.
                                </p>
                         
                                 <p style="font-size: 14px; color: #fde047;">Ako nisi zatražio/la izmenu lozinke, slobodno ignoriši ovaj mejl. Tvoja lozinka ostaje netaknuta.</p>
                                 
                                 <hr style="border: none; border-top: 1px solid #047857; margin: 20px 0;" />
                                 
                                 <p style="font-size: 12px; color: #a7f3d0; word-break: break-all;">
                                     U slučaju da imaš problema sa dugmetom, kopiraj i zalepi sledeći URL u pretraživač:<br/>
                                     <a href="{resetUrl}" style="color: #60a5fa; text-decoration: underline;">{resetUrl}</a>
                                 </p>
                                 
                                 <p style="margin-bottom: 0;">Srdačan pozdrav,<br/><strong><span style="color: #f59e0b;">Code</span>WithMixx</strong></p>
                             </div>
                         </body>
                         </html>
                         """;

        var message = new EmailMessage
        {
            From = _fromEmail,
            To = email,
            HtmlBody = htmlBody,
            Subject = "Resetovanje lozinke"
        };
        
        await resend.EmailSendAsync(message);
    }
}