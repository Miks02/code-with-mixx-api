using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Infrastructure.Filters;

namespace CodeWithMixx.API.Infrastructure;

public static class EndpointMapper
{
    public static void MapEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/")
            .AddEndpointFilter<ValidationFilter>()
            .AddEndpointFilter<ProblemDetailsFilter>();

        var endpoints = typeof(Program).Assembly
            .GetTypes()
            .Where(t => !t.IsAbstract && t.IsClass && typeof(IEndpoint).IsAssignableFrom(t));

        foreach (var endpoint in endpoints)
        {
            var instance = Activator.CreateInstance(endpoint) as IEndpoint;
            instance?.MapEndpoint(group);
        }

    }
}