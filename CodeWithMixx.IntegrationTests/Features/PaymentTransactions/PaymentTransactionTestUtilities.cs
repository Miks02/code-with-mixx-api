using CodeWithMixx.API.Domain.Entities.Admins;
using CodeWithMixx.API.Domain.Entities.PaymentTransactions;
using CodeWithMixx.API.Domain.Entities.Reservations;
using CodeWithMixx.API.Domain.Entities.Students;
using CodeWithMixx.API.Domain.Entities.Subjects;
using CodeWithMixx.API.Domain.Entities.Users;
using CodeWithMixx.API.Infrastructure.Persistence;
using CodeWithMixx.IntegrationTests.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CodeWithMixx.IntegrationTests.Features.PaymentTransactions;

public static class PaymentTransactionTestUtilities
{
    public static async Task<string> CreateOtherStudentAsync(this IntegrationTestFactory factory)
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var studentUser = User.CreateUser("Other", "Student", $"{Guid.NewGuid()}@test.com", "0600000002");
        context.Users.Add(studentUser);
        context.Students.Add(Student.Create(studentUser.Id, null).Payload!);
        await context.SaveChangesAsync();

        return studentUser.Id;
    }

    public static async Task<int> CreateReservationAsync(this IntegrationTestFactory factory, string studentId)
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var adminUser = await context.Users.FirstAsync(u => u.Email == TestConstants.AdminEmail);

        if (!await context.Admins.AnyAsync(a => a.UserId == adminUser.Id))
            context.Admins.Add(Admin.Create(adminUser.Id));

        var subject = Subject.Create($"Subject {Guid.NewGuid()}", "Description").Payload!;
        context.Subjects.Add(subject);
        await context.SaveChangesAsync();

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
                    SubjectId = subject.Id,
                    Price = 1300m,
                    StartsAt = startsAt,
                    EndsAt = startsAt.AddMinutes(45),
                    ReservedAt = today
                }
            ]
        }, today).Payload!;

        context.Reservations.Add(reservation);
        await context.SaveChangesAsync();

        return reservation.Id;
    }

    public static async Task<int> RegisterPaymentAsync(this IntegrationTestFactory factory, int reservationId, decimal amount, DateTime transactionDate)
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var reservation = await context.Reservations
            .IgnoreQueryFilters()
            .FirstAsync(r => r.Id == reservationId);

        var transaction = reservation
            .RegisterPayment(amount, transactionDate, DateOnly.FromDateTime(DateTime.UtcNow))
            .Payload!;

        await context.SaveChangesAsync();

        return transaction.Id;
    }

    public static async Task<int> SubtractPaymentAsync(this IntegrationTestFactory factory, int reservationId, decimal amount, PaymentTransactionType paymentType, DateTime transactionDate)
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var reservation = await context.Reservations
            .IgnoreQueryFilters()
            .FirstAsync(r => r.Id == reservationId);

        var transaction = reservation
            .SubtractPayment(amount, transactionDate, paymentType, DateOnly.FromDateTime(DateTime.UtcNow), "Note")
            .Payload!;

        await context.SaveChangesAsync();

        return transaction.Id;
    }

    public static async Task ArchiveReservationAsync(this IntegrationTestFactory factory, int reservationId)
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var reservation = await context.Reservations
            .Include(r => r.Classes)
            .FirstAsync(r => r.Id == reservationId);

        reservation.DeleteClassReservation();

        await context.SaveChangesAsync();
    }

    public static async Task<Reservation> FindReservationAsync(this IntegrationTestFactory factory, int reservationId)
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        return await context.Reservations
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstAsync(r => r.Id == reservationId);
    }
}
