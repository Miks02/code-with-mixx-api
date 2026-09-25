using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace CodeWithMixx.API.Infrastructure.Security;

public class InviteTokenProviderOptions : DataProtectionTokenProviderOptions
{
    public InviteTokenProviderOptions()
    {
        Name = "InviteTokenProvider";
        TokenLifespan = TimeSpan.FromDays(7);
    }
} 
public class InviteTokenProvider<TUser> : DataProtectorTokenProvider<TUser>
    where TUser : class
{
    public InviteTokenProvider(
        IDataProtectionProvider dataProtectionProvider,
        IOptions<InviteTokenProviderOptions> options,
        ILogger<DataProtectorTokenProvider<TUser>> logger)
        : base(dataProtectionProvider, options, logger) { }
}