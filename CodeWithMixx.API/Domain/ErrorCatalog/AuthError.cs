using CodeWithMixx.API.Common.Result;

namespace CodeWithMixx.API.Domain.ErrorCatalog;

public class AuthError
{
    public static Error RegistrationFailed(string message = "Unexpected error happened during registration")
        => new("Auth.RegistrationFailed", message, ErrorType.Unauthorized);
        
    public static Error LoginFailed(string message = "Unexpected error happened during login")
        => new("Auth.LoginFailed", message, ErrorType.Unauthorized);
        
    public static Error PasswordError(string message = "Error occurred while trying to assign password to the user")
        => new("Auth.InvalidCredentials", message);

    public static Error InvalidCurrentPassword(
        string message = "Entered password does not match the current password")
        => new("Auth.InvalidCurrentPassword", message, ErrorType.Validation);

    public static Error PasswordTooShort(string message = "Password is too short")
        => new("Auth.PasswordTooShort", message, ErrorType.Validation);

    public static Error PasswordRequiresDigit(string message = "Password must contain at least one digit ('0'-'9')")
        => new("Auth.PasswordRequiresDigit", message, ErrorType.Validation);

    public static Error PasswordRequiresUpper(string message = "Password must contain at least one uppercase letter ('A'-'Z')")
        => new("Auth.PasswordRequiresUpper", message, ErrorType.Validation);

    public static Error PasswordRequiresNonAlphanumeric(string message = "Password must contain at least one special character")
        => new("Auth.PasswordRequiresNonAlphanumeric", message, ErrorType.Validation);

    public static Error AccountLocked(string message = "Account is locked")
        => new("Auth.AccountLocked", message, ErrorType.Unauthorized);

    public static Error JwtError(string message = "Error happened while trying to assign refresh token to the user")
        => new("Auth.JwtError", message, ErrorType.Unauthorized);

    public static Error ExpiredToken(string message = "Refresh token has expired")
        => new("Auth.ExpiredToken", message, ErrorType.Unauthorized);
    public static Error SecurityBreach(string message = "Possible security breach detected")
        => new("Auth.SecurityBreach", "Possible security breach detected. All tokens have been revoked", ErrorType.Unauthorized);
}