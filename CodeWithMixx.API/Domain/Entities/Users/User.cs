using CodeWithMixx.API.Common.Results;
using CodeWithMixx.API.Domain.Entities.Admins;
using CodeWithMixx.API.Domain.Entities.Students;
using Microsoft.AspNetCore.Identity;

namespace CodeWithMixx.API.Domain.Entities.Users;

public class User : IdentityUser, IAuditable, ISoftDeletable
{
    public string FirstName { get; private set; } = null!;
    public string LastName { get; private set; } = null!;

    public DateTime? LastLoginAt { get; private set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get;  set; }
    public DateTime? DeletedAt { get; set; }
    public AccountStatus AccountStatus { get; private set; }

    public Student? Student { get; }
    public Admin? Admin { get; }

    public static User CreateUser(string firstName, string lastName, string email, string phoneNumber)
    {
        return new User
        {
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            UserName = email,
            PhoneNumber = phoneNumber,
            CreatedAt = DateTime.UtcNow,
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
            CreatedAt = DateTime.UtcNow,
            AccountStatus = AccountStatus.Active
        };
    }
    
    public void UpdateLastLogin()
    {
        LastLoginAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public Result ActivateAccount()
    {
        if(PasswordHash is null) 
            return Result.Failure(UserError.CannotActivateWithNullPassword(Id));
        
        if(IsDeleted) 
            return Result.Failure(UserError.CannotChangeStatusForDeletedUser(Id));
        
        if(AccountStatus == AccountStatus.Active) 
            return Result.Failure(UserError.AlreadyActivated(Id));
        
        AccountStatus = AccountStatus.Active;
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }
    
    public Result DeactivateAccount()
    {
        if(IsDeleted) 
            return Result.Failure(UserError.CannotChangeStatusForDeletedUser(Id));
        
        if(AccountStatus == AccountStatus.Deactivated) 
            return Result.Failure(UserError.AlreadyDeactivated(Id));
        
        AccountStatus = AccountStatus.Deactivated;
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }

    public void DeleteUser()
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        Email = "deleted_" + Id + "@deleted.invalid";
        UserName = "deleted_" + Id + "@deleted.invalid";
        PasswordHash = null;
        PhoneNumber = null;
        FirstName = "Deleted";
        LastName = "Deleted";
        AccountStatus = AccountStatus.Deleted;
        Student?.Delete();
    }


}