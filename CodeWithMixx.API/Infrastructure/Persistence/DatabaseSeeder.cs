using CodeWithMixx.API.Domain.Entities.Admins;
using CodeWithMixx.API.Domain.Entities.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CodeWithMixx.API.Infrastructure.Persistence;

public class DatabaseSeeder(
    RoleManager<IdentityRole> roleManager, 
    UserManager<User> userManager,
    ILogger<DatabaseSeeder> logger,
    IConfiguration configuration)
{
    public async Task SeedRolesAsync()
    {
        string[] roles = ["Admin", "Student"];
        string[] existingRoles = await roleManager.Roles.Select(x => x.Name!).ToArrayAsync();
        List<string> createdRoles = [];

        foreach (var role in roles)
        {
            if (existingRoles.Contains(role))
                continue;
            
            var roleResult = await roleManager.CreateAsync(new IdentityRole(role));
            if (!roleResult.Succeeded)
            {
                var errors = roleResult.Errors.Select(x => x.Description).ToArray();
                throw new InvalidOperationException($"Failed to create a specified role: {role}\nErrors:" + string.Join(", ", errors));
            }
            
            createdRoles.Add(role);
        }

        if (createdRoles.Count == 0)
        {
            logger.LogInformation("No new roles were created, all predefined roles already exist");
            return;
        }
        
        logger.LogInformation("Created roles: {Roles}", string.Join(", ", createdRoles));

    }

    public async Task SeedAdminAsync()
    {
        var adminEmail = configuration["Admin:Email"]!.Trim();
        
        var existingAdmin = await userManager.FindByEmailAsync(adminEmail);
        
        if (existingAdmin is not null && !await userManager.IsInRoleAsync(existingAdmin, "Admin"))
            throw new InvalidOperationException($"A user with specified email '{adminEmail}' already exists and is not an administrator.");

        if (existingAdmin is not null)
        {
            logger.LogInformation("Administrator with email '{Email}' already exists, skipping creation", adminEmail);
            return;
        }
        
        var admin = new User
        {
            FirstName = configuration["Admin:FirstName"]!,
            LastName = configuration["Admin:LastName"]!,
            PhoneNumber = configuration["Admin:PhoneNumber"],
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true,
            PhoneNumberConfirmed = true,
        };
        
        var createResult = await userManager.CreateAsync(admin, configuration["Admin:Password"]!);

        if (!createResult.Succeeded)
        {
            var errors = createResult.Errors.Select(e => e.Description).ToArray();
            throw new InvalidOperationException($"Failed to create an administrator. Errors: {string.Join(", ", errors)}");        }
        
        var roleResult = await userManager.AddToRoleAsync(admin, "Admin");
        
        if (!roleResult.Succeeded)
        {
            var errors = roleResult.Errors.Select(e => e.Description).ToArray();
            throw new InvalidOperationException($"Failed to add an administrator to the 'Admin' role. Errors: {string.Join(", ", errors)}");
        }
        
        logger.LogInformation("An administrator with email '{email}' has been created successfully", adminEmail);
        
    }
}