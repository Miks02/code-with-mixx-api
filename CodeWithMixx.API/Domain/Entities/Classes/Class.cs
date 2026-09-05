using CodeWithMixx.API.Common.Results;
using CodeWithMixx.API.Domain.Entities.Reservations;
using CodeWithMixx.API.Domain.Entities.Subjects;

namespace CodeWithMixx.API.Domain.Entities.Classes
{
    public class Class 
    {
        public int Id { get; set; }
        public decimal Price { get; set; }
        public DateTime StartsAt { get; set; }
        public DateTime EndsAt { get; set; }

        public Reservation Reservation { get; set; } = null!;
        public int ReservationId { get; set; } 
        public Subject Subject { get; set; } = null!;
        public int SubjectId { get; set; }
        
        public static Result<Class> Create(int subjectId, decimal price, DateTime startsAt, DateTime endsAt)
        {
            if(subjectId <= 0)
                return Result<Class>.Failure(SubjectError.NotFound(subjectId));
            if(price <= 0)
                return Result<Class>.Failure(ClassError.InvalidPrice(price));
            if(startsAt >= endsAt)
                return Result<Class>.Failure(ClassError.InvalidSchedule(startsAt, endsAt));

            var newClass = new Class
            {
                SubjectId = subjectId,
                Price = price,
                StartsAt = startsAt,
                EndsAt = endsAt
            };
            
            return Result<Class>.Success(newClass);
        }
    }
}
