using System.Security.Claims;
using CodeWithMixx.API.Common.Interfaces;

namespace CodeWithMixx.API.Infrastructure.Security;

public class UserProvider(IHttpContextAccessor http) : IUserProvider
{
    public string GetUserId() 
        => http.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedAccessException("User is not authenticated");
   

    public string GetUserIpAddress() 
        => http.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "Unknown IP";


}