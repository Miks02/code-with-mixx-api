using System.Security.Claims;
using System.Text.Json.Serialization;
using CodeWithMixx.API.Infrastructure;
using Microsoft.AspNetCore.Http.Json;
using Scalar.AspNetCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog((context, config) => config.ReadFrom.Configuration(context.Configuration));


builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();
await app.MapSeeders();

app.UseForwardedHeaders();
app.UseExceptionHandler();

app.MapEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapScalarApiReference();
    app.MapOpenApi();
    app.UseCors("DevCors");
}
else
{
    app.UseCors("ProdCors");
}
app.UseRateLimiter();

app.UseSerilogRequestLogging(options =>
{
    options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms (User: {UserId})";

    options.EnrichDiagnosticContext = (context, httpContext) =>
    {
        var userId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "Anonymous";
        context.Set("UserId", userId);
    };
});

app.UseAuthentication();
app.UseAuthorization();

app.Run();
