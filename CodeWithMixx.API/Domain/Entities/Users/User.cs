using CodeWithMixx.API.Common.Results;
using CodeWithMixx.API.Domain.Entities.Admins;
using CodeWithMixx.API.Domain.Entities.RefreshTokens;
using CodeWithMixx.API.Domain.Entities.Students;
using Microsoft.AspNetCore.Identity;

namespace CodeWithMixx.API.Domain.Entities.Users;

public class User : IdentityUser, IAuditable, ISoftDeletable
{
    public string FirstName { get; private set; } = null!;
    public string LastName { get; private set; } = null!;

    public DateTime? LastLoginAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }
    public AccountStatus AccountStatus { get; private set; }

    public Student? Student { get; }
    public Admin? Admin { get; }

    private readonly List<RefreshToken> _refreshTokens = [];
    public IReadOnlyCollection<RefreshToken> RefreshTokens => _refreshTokens.AsReadOnly();

    private User() {}

    public static User CreateUser(string firstName, string lastName, string email, string phoneNumber)
    {
        return new User
        {
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            UserName = email,
            PhoneNumber = phoneNumber,
            EmailConfirmed = false,
            AccountStatus = AccountStatus.Pending
        };
    }

    public static User CreateAdmin(string firstName, string lastName, string email, string phoneNumber)
    {
        return new User
        {
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            UserName = email,
            EmailConfirmed = true,
            PhoneNumberConfirmed = true,
            PhoneNumber = phoneNumber,
            AccountStatus = AccountStatus.Active
        };
    }

    public Result ChangeName(string firstName, string lastName)
    {
        var trimmedFirstName = firstName.Trim();
        var trimmedLastName = lastName.Trim();

        if (trimmedFirstName.Length == 0)
            return Result.Failure(UserError.FirstNameRequired());

        if (trimmedFirstName.Length > UserConstants.MaxNameLength)
            return Result.Failure(UserError.FirstNameTooLong());

        if (trimmedLastName.Length == 0)
            return Result.Failure(UserError.LastNameRequired());

        if (trimmedLastName.Length > UserConstants.MaxNameLength)
            return Result.Failure(UserError.LastNameTooLong());

        FirstName = trimmedFirstName;
        LastName = trimmedLastName;
        return Result.Success();
    }
    
    public void UpdateLastLogin()
    {
        LastLoginAt = DateTime.UtcNow;
    }

    public Result ActivateAccount()
    {
        
        if(IsDeleted) 
            return Result.Failure(UserError.CannotChangeStatusForDeletedUser(Id));
        
        if(AccountStatus == AccountStatus.Active) 
            return Result.Failure(UserError.AlreadyActivated(Id));
        
        if (PasswordHash is null)
        {
            if (AccountStatus == AccountStatus.Pending)
                return Result.Failure(UserError.CannotActivateWithNullPassword(Id));

            AccountStatus = AccountStatus.Pending;
            return Result.Success();
        }

        AccountStatus = AccountStatus.Active;
        return Result.Success();
    }

    public void ConfirmEmail()
    {
        if (EmailConfirmed)
            return;

        EmailConfirmed = true;
    }
    
    public Result DeactivateAccount()
    {
        if(IsDeleted) 
            return Result.Failure(UserError.CannotChangeStatusForDeletedUser(Id));
        
        if(AccountStatus == AccountStatus.Deactivated) 
            return Result.Failure(UserError.AlreadyDeactivated(Id));
        
        AccountStatus = AccountStatus.Deactivated;
        return Result.Success();
    }

    public Result DeleteUser()
    {
        if (IsDeleted)
            return Result.Failure(UserError.AlreadyDeleted(Id));

        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
        Email = "deleted_" + Id + "@deleted.invalid";
        UserName = "deleted_" + Id + "@deleted.invalid";
        PasswordHash = null;
        PhoneNumber = null;
        FirstName = "Deleted";
        LastName = "Deleted";
        AccountStatus = AccountStatus.Deleted;
        Student?.Delete();
        return Result.Success();
    }


}