using CodeWithMixx.API.Common.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Npgsql;
using NSubstitute;
using Respawn;
using Testcontainers.PostgreSql;

namespace CodeWithMixx.IntegrationTests;

public class IntegrationTestFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _db = new PostgreSqlBuilder("postgres:17").Build();

    private Respawner _respawner = null!;
    private NpgsqlConnection _connection = null!;

    private IAuthEmailSender AuthEmailSender { get; } = Substitute.For<IAuthEmailSender>();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("ConnectionStrings:DefaultConnection", _db.GetConnectionString());
        builder.UseSetting("JwtConfig:Token", "test-secret-key-that-is-long-enough-1234567890");
        builder.UseSetting("JwtConfig:Issuer", "test");
        builder.UseSetting("JwtConfig:Audience", "test");
        builder.UseSetting("JwtConfig:ExpirationInMinutes", "15");
        builder.UseSetting("RefreshConfig:ExpirationInDays", "7");
        builder.UseSetting("RefreshConfig:RetentionThresholdInDays", "7");
        builder.UseSetting("RefreshConfig:CleanupIntervalInDays", "1");
        builder.UseSetting("CookieConfig:Domain", "localhost");
        builder.UseSetting("Admin:Email", "admin@test.com");
        builder.UseSetting("Admin:Password", "Admin123!");
        builder.UseSetting("Admin:FirstName", "Admin");
        builder.UseSetting("Admin:LastName", "Test");
        builder.UseSetting("Admin:PhoneNumber", "0600000000");

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IHostedService>();
            services.RemoveAll<IAuthEmailSender>();
            services.RemoveAll<IInquirySender>();
            services.AddSingleton(AuthEmailSender);
            services.AddSingleton(Substitute.For<IInquirySender>());
        });
    }

    public async Task InitializeAsync()
    {
        await _db.StartAsync();
        _ = Services;
        
        _connection = new NpgsqlConnection(_db.GetConnectionString());
        await _connection.OpenAsync();
        
        _respawner = await Respawner.CreateAsync(_connection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            SchemasToInclude = ["public"],
            TablesToIgnore = ["__EFMigrationsHistory", "Roles", "Users"]
        });
    }

    public Task ResetDatabaseAsync() => _respawner.ResetAsync(_connection);

    public HttpClient CreateHttpsClients() => CreateClient(new WebApplicationFactoryClientOptions
    {
        BaseAddress = new Uri("https://localhost"),
        HandleCookies = true
    });

    public new async Task DisposeAsync()
    {
        await _connection.DisposeAsync();
        await _db.DisposeAsync();
    }
}