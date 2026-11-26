using CodeWithMixx.API.Domain.Entities.Admins;
using CodeWithMixx.API.Domain.Entities.Reservations;
using CodeWithMixx.API.Domain.Entities.Students;
using CodeWithMixx.API.Domain.Entities.Subjects;
using CodeWithMixx.API.Domain.Entities.Users;
using CodeWithMixx.API.Infrastructure.Persistence;
using CodeWithMixx.IntegrationTests.Common;
using Microsoft.EntityFrameworkCore;
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

    public static async Task<Subject?> FindSubjectAsync(this IntegrationTestFactory factory, int subjectId)
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        return await context.Subjects
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == subjectId);
    }

    public static async Task CreateClassReservationAsync(this IntegrationTestFactory factory, int subjectId)
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var studentUser = User.CreateUser("Student", "Test", $"{Guid.NewGuid()}@test.com", "0600000001");
        context.Users.Add(studentUser);
        context.Students.Add(Student.Create(studentUser.Id, null).Payload!);
        await context.SaveChangesAsync();

        await factory.CreateClassReservationAsync(subjectId, studentUser.Id);
    }

    public static async Task CreateClassReservationAsync(this IntegrationTestFactory factory, int subjectId, string studentId)
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var adminUser = await context.Users.FirstAsync(u => u.Email == TestConstants.AdminEmail);

        var adminExists = await context.Admins.AnyAsync(a => a.UserId == adminUser.Id);
        if (!adminExists)
            context.Admins.Add(Admin.Create(adminUser.Id));

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var startsAt = DateTime.UtcNow.Date.AddDays(1).AddHours(10);

        var reservation = Reservation.CreateClassReservation(new ReservationCreateData
        {
            AdminId = adminUser.Id,
            StudentId = studentId,
            ReservedAt = today,
            Classes =
            [
                new ClassCreateData
                {
                    Name = "Test class",
                    SubjectId = subjectId,
                    Price = 1300m,
                    StartsAt = startsAt,
                    EndsAt = startsAt.AddMinutes(45),
                    ReservedAt = today
                }
            ]
        }, today).Payload!;

        context.Reservations.Add(reservation);
        await context.SaveChangesAsync();
    }
}
