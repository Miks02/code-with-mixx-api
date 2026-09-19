using CodeWithMixx.API.Common.Results;
using CodeWithMixx.API.Domain.Entities.Admins;
using CodeWithMixx.API.Domain.Entities.Classes;
using CodeWithMixx.API.Domain.Entities.Projects;
using CodeWithMixx.API.Domain.Entities.Students;

namespace CodeWithMixx.API.Domain.Entities.Reservations
{
    public class Reservation : IAuditable, ISoftDeletable
    {
        public int Id { get; private set; }
        public ReservationStatus ReservationStatus { get; private set; } = ReservationStatus.Confirmed;
        public PaymentStatus PaymentStatus { get; private set; } = PaymentStatus.Pending;

        public decimal TotalPrice { get; private set; }
        public decimal PaidAmount { get; private set; }
        public decimal DiscountRate { get; private set; }
        public decimal Bonus { get; private set; }

        public string? Notes { get; private set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }

        public Admin Admin { get; private set; } = null!;
        public string AdminId { get; private set; } = null!;

        public Student Student { get; private set; } = null!;
        public string StudentId { get; private set; } = null!;
        
        public ReservationType ReservationType { get; private set; }

        public ICollection<Class> Classes { get; private set; } = [];
        public ICollection<Project> Projects { get; private set; } = [];
        
        private Reservation() {}

        public static Result<Reservation> CreateClassReservation(ReservationCreateData data)
        {
            if (data.Classes.Count == 0)
                return Result<Reservation>.Failure(ReservationError.NoClassesProvided());
            
            if(data.TotalPrice is not null && data.TotalPrice < 0)
                return Result<Reservation>.Failure(ReservationError.InvalidTotalPrice(data.TotalPrice.Value));
            
            if(data.PaidAmount < 0)
                return Result<Reservation>.Failure(ReservationError.InvalidAmount(data.PaidAmount));
            
            var reservation = new Reservation
            {
                AdminId = data.AdminId,
                StudentId = data.StudentId,
                ReservationStatus = data.ReservationStatus,
                PaidAmount = data.PaidAmount,
                CreatedAt = DateTime.UtcNow,
                ReservationType = ReservationType.Class
            };

            reservation.TotalPrice = reservation.CalculateTotalPrice(data.TotalPrice);
            reservation.DiscountRate = reservation.CalculateDiscountRate(reservation.TotalPrice);
            reservation.Bonus = reservation.CalculateBonus(reservation.TotalPrice, reservation.PaidAmount);
            reservation.PaymentStatus = reservation.DeterminePaymentStatus(reservation.PaidAmount, reservation.TotalPrice);
            
            var classesResult = reservation.AddClasses(data.Classes);
            
            if (!classesResult.IsSuccess)
                return Result<Reservation>.Failure(classesResult.Errors[0]);
            
            return Result<Reservation>.Success(reservation);
        }
        
        public static Result<Reservation> CreateProjectReservation(ReservationCreateData data, IReadOnlyList<ProjectCreateData> projects)
        {
            if(projects.Count == 0)
                return Result<Reservation>.Failure(ReservationError.NoProjectsProvided());
            
            if(data.TotalPrice is not null && data.TotalPrice < 0)
                return Result<Reservation>.Failure(ReservationError.InvalidTotalPrice(data.TotalPrice.Value));
            
            if(data.PaidAmount < 0)
                return Result<Reservation>.Failure(ReservationError.InvalidAmount(data.PaidAmount));
            
            var reservation = new Reservation
            {
                AdminId = data.AdminId,
                StudentId = data.StudentId,
                ReservationStatus = data.ReservationStatus,
                TotalPrice = data.TotalPrice ?? 0,
                PaidAmount = data.PaidAmount,
                CreatedAt = DateTime.UtcNow,
                ReservationType = ReservationType.Project
            };
            
            var addProjectsResult = reservation.AddProjects(projects);
            
            if (!addProjectsResult.IsSuccess)
                return Result<Reservation>.Failure(addProjectsResult.Errors[0]);
            
            reservation.TotalPrice = reservation.CalculateTotalPrice(data.TotalPrice);
            reservation.DiscountRate = reservation.CalculateDiscountRate(reservation.TotalPrice);
            reservation.Bonus = reservation.CalculateBonus(reservation.TotalPrice, data.PaidAmount);
            reservation.PaymentStatus = reservation.DeterminePaymentStatus(data.PaidAmount, reservation.TotalPrice);

            
            return Result<Reservation>.Success(reservation);
        }
        
        public void UpdateReservationStatus(ReservationStatus newStatus)
        {
            ReservationStatus = newStatus;
            UpdatedAt = DateTime.UtcNow;
        }

        public Result UpdateTotalPrice(decimal totalPrice)
        {
            if (totalPrice < 0)
                return Result.Failure(ReservationError.InvalidTotalPrice(totalPrice));

            TotalPrice = totalPrice;
            DiscountRate = CalculateDiscountRate(totalPrice);
            Bonus = CalculateBonus(totalPrice, PaidAmount);
            PaymentStatus = DeterminePaymentStatus(PaidAmount, TotalPrice);
            UpdatedAt = DateTime.UtcNow;

            return Result.Success();
        }

        public Result RegisterPayment(decimal paidAmount)
        {
            if (paidAmount < 0)
                return Result.Failure(ReservationError.InvalidAmount(paidAmount));

            PaidAmount += paidAmount;

            PaymentStatus = DeterminePaymentStatus(PaidAmount, TotalPrice);
            
            DiscountRate = CalculateDiscountRate(TotalPrice);
            Bonus = CalculateBonus(TotalPrice, PaidAmount);

            UpdatedAt = DateTime.UtcNow;

            return Result.Success();
        }
        
        public Result SubtractPayment(decimal amount)
        {
            if (amount < 0)
                return Result.Failure(ReservationError.InvalidAmount(amount, "Amount to subtract cannot be negative."));

            if (amount > PaidAmount)
                return Result.Failure(ReservationError.InvalidAmount(amount, "Cannot subtract more than the paid amount."));

            PaidAmount -= amount;

            PaymentStatus = DeterminePaymentStatus(PaidAmount, TotalPrice);

            DiscountRate = CalculateDiscountRate(TotalPrice);
            Bonus = CalculateBonus(TotalPrice, PaidAmount);

            UpdatedAt = DateTime.UtcNow;

            return Result.Success();
        }
        
        public void UpdateNotes(string? notes)
        {
            Notes = notes;
            UpdatedAt = DateTime.UtcNow;
        }
        
        public void ChangeStudent(string studentId)
        {
            StudentId = studentId;
            UpdatedAt = DateTime.UtcNow;
        }

        public Result UpdateClasses(IReadOnlyList<ClassUpdateData> classesToUpdate)
        {
            if(ReservationType != ReservationType.Project)
                return Result.Failure(ReservationError.NotAClassReservation(Id));

            var invalidClassIds = classesToUpdate.Where(c => Classes.All(existingClass => existingClass.Id != c.Id)).Select(c => c.Id).ToList();
            if (invalidClassIds.Count != 0)
                return Result.Failure(ClassError.NotFound(invalidClassIds[0]));

            foreach (var classUpdate in classesToUpdate)
            {
                var existingClass = Classes.First(c => c.Id == classUpdate.Id);
                var updateResult = existingClass.Update(classUpdate.SubjectId, classUpdate.Price, classUpdate.StartsAt, classUpdate.EndsAt);

                if (!updateResult.IsSuccess)
                    return Result.Failure(updateResult.Errors[0]);
            }
            
            TotalPrice = CalculateTotalPrice(TotalPrice);
            DiscountRate = CalculateDiscountRate(TotalPrice);
            Bonus = CalculateBonus(TotalPrice, PaidAmount);
            PaymentStatus = DeterminePaymentStatus(PaidAmount, TotalPrice);
            UpdatedAt = DateTime.UtcNow;

            return Result.Success();
        }
        
        public Result AddClasses(IReadOnlyList<ClassCreateData> classesToAdd)
        {
            if(ReservationType != ReservationType.Class)
                return Result.Failure(ReservationError.NotAClassReservation(Id));
                                                                            
            foreach (var classData in classesToAdd)
            {
                var classResult = Class.Create(classData.SubjectId, classData.Price, classData.StartsAt, classData.EndsAt);

                if (!classResult.IsSuccess)
                    return Result.Failure(classResult.Errors[0]);

                Classes.Add(classResult.Payload!);
            }

            return Result.Success();
        }

        public Result AddProjects(IReadOnlyList<ProjectCreateData> projectsToAdd)
        {
            if(ReservationType != ReservationType.Project)
                return Result.Failure(ReservationError.NotAProjectReservation(Id));

            foreach (var projectData in projectsToAdd)
            {
                var projectResult = Project.Create(projectData);
                if (!projectResult.IsSuccess)
                    return Result.Failure(projectResult.Errors[0]);

                Projects.Add(projectResult.Payload!);
            }
            
            return Result.Success();
        }
        
        public Result DeleteClassReservation()
        {
            if(ReservationType != ReservationType.Class)
                return Result.Failure(ReservationError.NotAClassReservation(Id));
            
            foreach (var @class in Classes)
            {
                var deleteResult = @class.Delete(); 
                
                if(!deleteResult.IsSuccess)
                    return deleteResult;
            }
            var result = Delete();
            if(!result.IsSuccess)
                return result;
            
            return Result.Success();
        }
        
        public Result DeleteProjectReservation()
        {
            if(ReservationType != ReservationType.Project)
                return Result.Failure(ReservationError.NotAProjectReservation(Id));
            
            foreach (var project in Projects)  
            {
                var deleteProjectResult = project.Delete(); 
                
                if(!deleteProjectResult.IsSuccess)
                    return deleteProjectResult;
            }
            var result = Delete();
            if(!result.IsSuccess)
                return result;
            
            return Result.Success();
        }
        
        public bool RequiresHistoryRetention()
            => ReservationStatus == ReservationStatus.Completed
               || PaymentStatus != PaymentStatus.Pending
               || Classes.Any(c => c.StartsAt <= DateTime.UtcNow)
               || Projects.Any(p => p.StartDate <= DateTime.UtcNow);
        
        private PaymentStatus DeterminePaymentStatus(decimal paidAmount, decimal totalPrice)
        {
            var lastItem = ReservationType == ReservationType.Class 
                ? Classes.Select(c => c.EndsAt).Last() 
                : Projects.Select(p => p.EndDate).Last();
            
            return paidAmount switch
            {
                var amount when amount >= totalPrice => PaymentStatus.Paid,
                var amount when amount < totalPrice && lastItem < DateTime.UtcNow => PaymentStatus.Overdue,
                var amount when amount > 0 && amount < totalPrice => PaymentStatus.PartiallyPaid,
                _ => PaymentStatus.Pending
            };
        }

        private decimal CalculateTotalPrice(decimal? totalPrice)
        {
            if (totalPrice is not null)
                return totalPrice.Value;

            return ReservationType == ReservationType.Class 
                ? Classes.Sum(c => c.Price) 
                : Projects.Sum(p => p.Price);
        }
        
        private decimal CalculateDiscountRate(decimal requestedTotalPrice)
        {
            var defaultTotal = ReservationType == ReservationType.Class 
                ? Classes.Sum(c => c.Price) 
                : Projects.Sum(p => p.Price);
            
            if (defaultTotal <= 0 || defaultTotal < requestedTotalPrice)
                return 0;
        
            var discountRate = (defaultTotal - requestedTotalPrice) / defaultTotal * 100;
            return Math.Round(discountRate, 2);
        }

        private decimal CalculateBonus(decimal totalPrice, decimal paidAmount)
        {
            if (totalPrice >= paidAmount)
                return 0;

            var bonus = paidAmount - totalPrice;
        
            return Math.Round(bonus, 2);
        }

        private Result Delete()
        {
            if(IsDeleted) 
                return Result.Failure(ReservationError.AlreadyDeleted(Id));

            IsDeleted = true;
            DeletedAt = DateTime.UtcNow;
            return Result.Success();
        }
    }
}
