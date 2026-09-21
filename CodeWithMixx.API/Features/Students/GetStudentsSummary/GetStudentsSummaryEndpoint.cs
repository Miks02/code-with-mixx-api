using CodeWithMixx.API.Common.Interfaces;

namespace CodeWithMixx.API.Features.Students.GetStudentsSummary;

public class GetStudentsSummaryEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/admin/students/summary", async (
                    [AsParameters] GetStudentsSummaryRequest request,
                    IHandler<GetStudentsSummaryRequest, GetStudentsSummaryResponse> handler,
                    CancellationToken ct) 
            => await handler.HandleAsync(request, ct))
            .RequireAuthorization("AdminOnly")
            .WithTags("Students")
            .Produces<GetStudentsSummaryResponse>();
    }
}