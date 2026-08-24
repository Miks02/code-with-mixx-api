using Microsoft.EntityFrameworkCore;

namespace CodeWithMixx.API.Infrastructure.Persistence;

public static class PersistenceRegistration
{
    public static void AddPersistence(this IServiceCollection services, string connectionString)
    {
        var railwayDbUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
        if (!string.IsNullOrEmpty(railwayDbUrl))
        {
            var databaseUri = new Uri(railwayDbUrl);
            var host = databaseUri.Host;
            var port = databaseUri.Port;
            var database = databaseUri.AbsolutePath.TrimStart('/');
        
            string username = "";
            string password = "";

            if (!string.IsNullOrEmpty(databaseUri.UserInfo))
            {
                var userInfo = databaseUri.UserInfo.Split(':');
                username = userInfo[0];
                password = userInfo.Length > 1 ? userInfo[1] : "";
            }

            connectionString = $"Host={host};Port={port};Database={database};";
        
            if (!string.IsNullOrEmpty(username)) connectionString += $"Username={username};";
            if (!string.IsNullOrEmpty(password)) connectionString += $"Password={password};";
        
            connectionString += "SSL Mode=Require;Trust Server Certificate=true;";
        }
        
        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseNpgsql(connectionString);
        });

        services.AddScoped<DatabaseSeeder>();
    }
}