using CodeWithMixx.API.Domain.Entities.Subjects;
using CodeWithMixx.API.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace CodeWithMixx.IntegrationTests.Features.Subjects;

public static class SubjectTestUtilities
{
    public static async Task<Subject> CreateSubjectAsync(this IntegrationTestFactory factory, string subjectName, string subjectDescription, bool isArchived)
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        
        var subject = Subject.Create(subjectName, subjectDescription).Payload!;
        
        if(isArchived)
            subject.Archive();
        
        await context.Subjects.AddAsync(subject);
        await context.SaveChangesAsync();
        
        return subject;
    }
}