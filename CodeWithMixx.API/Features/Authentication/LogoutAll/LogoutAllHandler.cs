using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Common.Results;

namespace CodeWithMixx.API.Features.Authentication.LogoutAll;

public class LogoutAllHandler(
    IUserProvider userProvider,
    ICookieProvider cookieProvider,
    ITokenService tokenService) : IHandler<LogoutAllRequest, Result>
{
    public async Task<Result> HandleAsync(LogoutAllRequest request, CancellationToken ct = default)
    {
        await tokenService.RevokeAllUserTokensAsync(userProvider.GetUserId());
        cookieProvider.RemoveAuthCookies();
        
        return Result.Success();
    }
}