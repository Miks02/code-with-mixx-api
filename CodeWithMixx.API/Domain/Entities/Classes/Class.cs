using CodeWithMixx.API.Common.Results;
using CodeWithMixx.API.Domain.Entities.Reservations;
using CodeWithMixx.API.Domain.Entities.Subjects;

namespace CodeWithMixx.API.Domain.Entities.Classes
{
    public class Class : IAuditable, ISoftDeletable
    {
        public int Id { get; private set; }
        public decimal Price { get; private set; }
        public DateTime StartsAt { get; private set; }
        public DateTime EndsAt { get; private set; }

        public Reservation Reservation { get; private set; } = null!;
        public int ReservationId { get; private set; } 
        public Subject Subject { get; private set; } = null!;
        public int SubjectId { get; private set; }
        
        public bool IsDeleted { get; private set; }
        public DateTime? DeletedAt { get; private set; }
        
        public DateTime CreatedAt { get; private set; }
        public DateTime? UpdatedAt { get; private set; }
        
        
        private Class() {}

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
                EndsAt = endsAt,
                CreatedAt = DateTime.UtcNow
            };
            
            return Result<Class>.Success(newClass);
        }
        
        public Result<Class> Update(int subjectId, decimal price, DateTime startsAt, DateTime endsAt)
        {
            if(subjectId <= 0)
                return Result<Class>.Failure(SubjectError.NotFound(subjectId));
            if(price <= 0)
                return Result<Class>.Failure(ClassError.InvalidPrice(price));
            if(startsAt >= endsAt)
                return Result<Class>.Failure(ClassError.InvalidSchedule(startsAt, endsAt));

            SubjectId = subjectId;
            Price = price;
            StartsAt = startsAt;
            EndsAt = endsAt;
            UpdatedAt = DateTime.UtcNow;

            return Result<Class>.Success(this);
        }

        public Result Delete()
        {
            if(IsDeleted)
                return Result.Failure(ClassError.AlreadyDeleted(Id));
            
            IsDeleted = true;
            DeletedAt = DateTime.UtcNow;

            return Result.Success();
        }

        public Result Restore()
        {
            if(!IsDeleted)
                return Result.Failure(ClassError.NotArchived(Id));

            IsDeleted = false;
            DeletedAt = null;

            return Result.Success();
        }
    }
}
