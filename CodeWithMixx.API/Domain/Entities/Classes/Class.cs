using CodeWithMixx.API.Common.Results;
using CodeWithMixx.API.Domain.Entities.Reservations;
using CodeWithMixx.API.Domain.Entities.Subjects;

namespace CodeWithMixx.API.Domain.Entities.Classes
{
    public class Class : IAuditable, ISoftDeletable
    {
        public int Id { get; private set; }
        public string Name { get; private set; } = null!;
        public decimal Price { get; private set; }
        public DateTime StartsAt { get; private set; }
        public DateTime EndsAt { get; private set; }
        public DateOnly ReservedAt { get; private set; }

        public Reservation Reservation { get; private set; } = null!;
        public int ReservationId { get; private set; } 
        public Subject Subject { get; private set; } = null!;
        public int SubjectId { get; private set; }
        
        public bool IsDeleted { get; private set; }
        public DateTime? DeletedAt { get; private set; }
        
        public DateTime CreatedAt { get; private set; }
        public DateTime? UpdatedAt { get; private set; }
        
        private Class() {}

        public static Result<Class> Create(string name, int subjectId, decimal price, DateTime startsAt, DateTime endsAt, DateOnly reservedAt)
        {
            var validationResult = Validate(name, subjectId, price, startsAt, endsAt, reservedAt);
            if(!validationResult.IsSuccess)
                return Result<Class>.Failure(validationResult.Errors[0]);
            
            var newClass = new Class
            {
                Name = name,
                SubjectId = subjectId,
                Price = price,
                StartsAt = startsAt,
                EndsAt = endsAt,
                ReservedAt = reservedAt
            };
            
            return Result<Class>.Success(newClass);
        }
        
        public Result<Class> Update(string name, int subjectId, decimal price, DateTime startsAt, DateTime endsAt, DateOnly reservedAt)
        {
            var validationResult = Validate(name, subjectId, price, startsAt, endsAt, reservedAt);
            if(!validationResult.IsSuccess)
                return Result<Class>.Failure(validationResult.Errors[0]);

            Name = name;
            SubjectId = subjectId;
            Price = price;
            StartsAt = startsAt;
            EndsAt = endsAt;
            ReservedAt = reservedAt;

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
        
        private static Result Validate(string name, int subjectId, decimal price, DateTime startsAt, DateTime endsAt, DateOnly reservedAt)
        {
            if(string.IsNullOrWhiteSpace(name))
                return Result.Failure(ClassError.EmptyName());
            if(name.Length > ClassConstants.MaxNameLength)
                return Result.Failure(ClassError.NameTooLong(name));
            if(subjectId <= 0)
                return Result<Class>.Failure(SubjectError.NotFound(subjectId));
            if(price <= 0)
                return Result<Class>.Failure(ClassError.InvalidPrice(price));
            if(startsAt >= endsAt)
                return Result<Class>.Failure(ClassError.InvalidSchedule(startsAt, endsAt));
            if ((endsAt - startsAt).TotalMinutes > ClassConstants.MaxDurationInMinutes)
                return Result<Class>.Failure(ClassError.InvalidDuration());
            if(reservedAt > DateOnly.FromDateTime(startsAt))
                return Result<Class>.Failure(ClassError.InvalidReservationDate());
            
            return Result.Success();
        }
    }
}
