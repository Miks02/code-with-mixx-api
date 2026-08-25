using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Domain.Entities.RefreshTokens;
using CodeWithMixx.API.Domain.Entities.Users;
using CodeWithMixx.API.Features.Authentication.Common;
using CodeWithMixx.API.Infrastructure.Exceptions;
using CodeWithMixx.API.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace CodeWithMixx.API.Infrastructure.Security;

public class TokenService(
    IConfiguration configuration, 
    AppDbContext context,
    UserManager<User> userManager,
    IUserProvider userProvider) : ITokenService
{
    public async Task<TokenResponseDto> AssignAuthTokens(User user)
    {
        var refreshToken = await AssignRefreshToken(user);
        var accessToken = await GenerateAccessToken(user);
        
        return new TokenResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken
        };
    }
    
    public async Task RevokeAllUserTokensAsync(string userId)
    {
        try
        {
            await context.RefreshTokens
                .Where(rt => rt.UserId == userId && DateTime.UtcNow < rt.ExpiresAt && !rt.RevokedAt.HasValue)
                .ExecuteUpdateAsync(rt => rt
                    .SetProperty(r => r.RevokedAt, DateTime.UtcNow)
                    .SetProperty(r => r.UpdatedAt, DateTime.UtcNow));
        }
        catch (DbUpdateException ex)
        {
            throw new SecurityDbUpdateException(userId, "An error occurred while revoking user tokens in the database.", ex);
        }
    }
    
    public string HashRefreshToken(string refreshToken)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(refreshToken);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }
    
    private async Task<string> GenerateAccessToken(User user)
    {
        var secretKey = configuration["JwtConfig:Token"]!;
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        
        var signingCreds = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        
        var roles = await userManager.GetRolesAsync(user);
        
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity([
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email!),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("lastName", user.LastName)
            ]),
            Expires = DateTime.UtcNow.AddMinutes(15),
            SigningCredentials = signingCreds,
            Issuer = configuration["JwtConfig:Issuer"],
            Audience = configuration["JwtConfig:Audience"]
        };
        
        var roleClaims = roles.Select(role => new Claim(ClaimTypes.Role, role));
        tokenDescriptor.Subject.AddClaims(roleClaims);
        
        var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
    
    private string CreateRawRefreshToken() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

    private async Task<string> AssignRefreshToken(User user)
    {
        var refreshToken = CreateRawRefreshToken();
        var refreshTokenHash = HashRefreshToken(refreshToken);
        
        var newRefreshToken = RefreshToken.CreateRefreshToken(
            tokenHash: refreshTokenHash,
            userId: user.Id,
            userIp: userProvider.GetUserIpAddress(),
            expiresAt: DateTime.UtcNow.AddDays(configuration.GetValue<int>("RefreshConfig:ExpirationInDays"))
        );
        
        context.RefreshTokens.Add(newRefreshToken);
        await context.SaveChangesAsync();

        return refreshToken;
    }
    
    
}