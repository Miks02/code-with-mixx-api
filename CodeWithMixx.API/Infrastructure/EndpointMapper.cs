using CodeWithMixx.API.Common.Markers;

namespace CodeWithMixx.API.Infrastructure;

public static class EndpointMapper
{
    public static void MapEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/");

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