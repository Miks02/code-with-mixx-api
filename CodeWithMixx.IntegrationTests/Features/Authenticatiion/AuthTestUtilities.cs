using CodeWithMixx.API.Common.Results;
using CodeWithMixx.API.Domain.Entities.Students;
using CodeWithMixx.API.Domain.Entities.Users;
using CodeWithMixx.API.Infrastructure.Persistence;
using CodeWithMixx.IntegrationTests.Common;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CodeWithMixx.IntegrationTests.Features.Authenticatiion;

public static class AuthTestUtilities
{
    public static async Task<User> GetOrCreateStudentAsync(this IntegrationTestFactory factory)
    {
        using var scope = factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var existingStudent = await userManager.FindByEmailAsync(TestConstants.StudentEmail);

        if (existingStudent is not null)
            return existingStudent;
        
        var user = User.CreateUser(TestConstants.StudentFirstName, TestConstants.StudentLastName, TestConstants.StudentEmail, TestConstants.StudentLastName);
        
        await userManager.CreateAsync(user, TestConstants.StudentPassword);
        await userManager.AddToRoleAsync(user, "Student");

        var student = Student.Create(user.Id, TestConstants.StudentUniversity);
        context.Add(student.Payload!);

        await context.SaveChangesAsync();
        
        return user;
    }
    
    public static async Task DeactivateStudentAsync(this IntegrationTestFactory factory, string userId)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var user = await context.Users.FirstOrDefaultAsync(u => u.Id == userId);

        if (user is null)
            throw new KeyNotFoundException("Account deactivation failed. User not found.");
        
        user.DeactivateAccount();
        await context.SaveChangesAsync();
    }
}