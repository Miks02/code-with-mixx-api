namespace CodeWithMixx.API.Common.Interfaces;

public interface IUserProvider
{
    string GetUserId();
    string GetUserIpAddress();
}