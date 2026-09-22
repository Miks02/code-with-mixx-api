using CodeWithMixx.API.Common.Result;

namespace CodeWithMixx.API.Domain.Entities.Users;

public class UserError
{
    public static Error EmailAlreadyExists(string email = "")
    {
        string message = string.IsNullOrWhiteSpace(email)
            ? "Email is taken"
            : $"Email '{email}' is taken";
            
        return new Error("User.EmailAlreadyExists", message, ErrorType.Conflict);
    }

    public static Error UsernameAlreadyExists(string username = "")
    {
        string message = string.IsNullOrWhiteSpace(username)
            ? "Username is taken"
            : $"Username '{username}' is taken";

        return new Error("User.UsernameAlreadyExists", message, ErrorType.Conflict);
    }

    public static Error NotFound(string identifier = "")
    {
        string message = string.IsNullOrWhiteSpace(identifier)
            ? "User not found"
            : $"User with identifier '{identifier}' is not found";

        return new Error("User.NotFound", message, ErrorType.NotFound);
    }
    
    public static Error CannotChangeStatusForDeletedUser(string identifier)
        => new(
            "User.CannotChangeStatusForDeletedUser", 
            $"User with identifier '{identifier}' cannot have his status changed status because he is deleted", ErrorType.Conflict);
    
    public static Error CannotActivateWithNullPassword(string identifier) 
        => new("User.CannotActivateWithNullPassword", $"User with identifier '{identifier}' cannot be activated because it has no password", ErrorType.Conflict);
    
    public static Error AlreadyActivated(string identifier) 
        => new("User.AlreadyActivated", $"User with identifier '{identifier}' is already activated", ErrorType.Conflict);
    
    public static Error AlreadyDeactivated(string identifier) 
        => new("User.AlreadyDeactivated", $"User with identifier '{identifier}' is already deactivated", ErrorType.Conflict);
}