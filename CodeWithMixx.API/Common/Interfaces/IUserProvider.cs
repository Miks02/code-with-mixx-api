namespace CodeWithMixx.API.Common.Interfaces;

public interface IUserProvider
{
    string GetUserId();
    string GetUserAgent();
    string GetUserIpAddress();
    IReadOnlyList<string> GetUserRoles();
}