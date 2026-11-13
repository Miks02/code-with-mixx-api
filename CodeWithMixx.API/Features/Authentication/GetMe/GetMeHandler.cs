using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Common.Results;
using CodeWithMixx.API.Domain.Entities.Users;
using CodeWithMixx.API.Features.Authentication.Common;
using CodeWithMixx.API.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UAParser;

namespace CodeWithMixx.API.Features.Authentication.GetMe;

public class GetMeHandler(IUserProvider userProvider, Parser uaParser, AppDbContext context) 
    : IHandler<GetMeRequest, Result<GetMeResponse>>
{
    public async Task<Result<GetMeResponse>> HandleAsync(GetMeRequest request, CancellationToken ct = default)
    {
        var currentSession = userProvider.GetSessionId();
        
        var userDetails = await context.Users
            .Include(u => u.RefreshTokens)
            .Where(u => u.Id == userProvider.GetUserId())
            .Select(u => new GetMeResponse
            {
                FirstName = u.FirstName,
                LastName = u.LastName,
                Email = u.Email!,
                PhoneNumber = u.PhoneNumber!,
                AccountStatus = u.AccountStatus,
                LastLogin = (DateTime) u.LastLoginAt!,
                Roles = userProvider.GetUserRoles(),
                ActiveSessions = u.RefreshTokens
                    .Where(rt => rt.RevokedAt == null && rt.ExpiresAt > DateTime.UtcNow)
                    .Select(rt => new ActiveSessionItem
                    {
                        SessionId = rt.SessionId,
                        ActiveSince = rt.CreatedAt,
                        Browser = uaParser.Parse(rt.UserAgent).UA.Family,
                        Os = uaParser.Parse(rt.UserAgent).OS.Family,
                        Device = SessionDeviceResolver.Resolve(uaParser.Parse(rt.UserAgent)),
                        IsCurrentSession = rt.SessionId.ToString() == currentSession
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync(ct);
        
        if(userDetails is null)
            return Result<GetMeResponse>.Failure(UserError.NotFound(userProvider.GetUserId()));
        
        return Result<GetMeResponse>.Success(userDetails);
    }
}
