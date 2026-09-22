using CodeWithMixx.API.Common.Results;
using CodeWithMixx.API.Domain.Entities.Admins;
using CodeWithMixx.API.Domain.Entities.Students;
using Microsoft.AspNetCore.Identity;

namespace CodeWithMixx.API.Domain.Entities.Users;

public class User : IdentityUser, IAuditable, ISoftDeletable
{
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;

    public DateTime? LastLoginAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public AccountStatus AccountStatus { get; set; }

    public Student? Student { get; set; }
    public Admin? Admin { get; set; }

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
    
    public void UpdateLastLogin()
    {
        LastLoginAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public Result ActivateAccount(AccountStatus status)
    {
        if(PasswordHash is null) 
            return Result.Failure(UserError.CannotActivateWithNullPassword(Id));
        
        if(IsDeleted) 
            return Result.Failure(UserError.CannotActivateDeletedUser(Id));
        
        if(AccountStatus == AccountStatus.Active) 
            return Result.Failure(UserError.AlreadyActivated(Id));
        
        AccountStatus = status;
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