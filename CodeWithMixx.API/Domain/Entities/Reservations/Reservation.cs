using CodeWithMixx.API.Common.Results;
using CodeWithMixx.API.Domain.Entities.Admins;
using CodeWithMixx.API.Domain.Entities.Classes;
using CodeWithMixx.API.Domain.Entities.PaymentTransactions;
using CodeWithMixx.API.Domain.Entities.Projects;
using CodeWithMixx.API.Domain.Entities.Students;
using CodeWithMixx.API.Domain.ValueObjects.Monies;

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
        
        public decimal RemainingAmount => Math.Max(0, TotalPrice - PaidAmount);

        public string? Notes { get; private set; }

        public DateOnly ReservedAt { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime? UpdatedAt { get; private set; }
        public bool IsDeleted { get; private set; }
        public DateTime? DeletedAt { get; private set; }

        public Admin Admin { get; private set; } = null!;
        public string AdminId { get; private set; } = null!;

        public Student Student { get; private set; } = null!;
        public string StudentId { get; private set; } = null!;
        
        public ReservationType ReservationType { get; private set; }

        private readonly List<Class> _classes = [];
        private readonly List<Project> _projects = [];
        private readonly List<PaymentTransaction> _paymentTransactions = [];
        
        public IReadOnlyCollection<Class> Classes => _classes.AsReadOnly();
        public IReadOnlyCollection<Project> Projects => _projects.AsReadOnly();
        public IReadOnlyCollection<PaymentTransaction> PaymentTransactions => _paymentTransactions.AsReadOnly();
        
        private Reservation() {}

        public static Result<Reservation> CreateClassReservation(ReservationCreateData data, decimal paidAmount, DateOnly today)
        {
            if (data.Classes.Count == 0)
                return Result<Reservation>.Failure(ReservationError.NoClassesProvided());
            
            if(data.TotalPrice is not null && data.TotalPrice < 0)
                return Result<Reservation>.Failure(ReservationError.InvalidTotalPrice(data.TotalPrice.Value));
            
            if(paidAmount < 0)
                return Result<Reservation>.Failure(ReservationError.InvalidAmount(paidAmount));
            
            var reservation = new Reservation
            {
                AdminId = data.AdminId,
                StudentId = data.StudentId,
                ReservationStatus = data.ReservationStatus,
                PaidAmount = paidAmount,
                ReservationType = ReservationType.Class
            };

            var classesResult = reservation.AddClasses(data.Classes);
            
            if (!classesResult.IsSuccess)
                return Result<Reservation>.Failure(classesResult.Errors[0]);
            
            reservation.TotalPrice = reservation.CalculateTotalPrice(data.TotalPrice);
            reservation.DiscountRate = reservation.CalculateDiscountRate(reservation.TotalPrice);
            reservation.Bonus = reservation.CalculateBonus(reservation.TotalPrice, reservation.PaidAmount);
            reservation.PaymentStatus = reservation.DeterminePaymentStatus(reservation.PaidAmount, reservation.TotalPrice, today);

            
            return Result<Reservation>.Success(reservation);
        }
        
        public static Result<Reservation> CreateProjectReservation(ReservationCreateData data, IReadOnlyList<ProjectCreateData> projects, DateOnly today)
        {
            if(projects.Count == 0)
                return Result<Reservation>.Failure(ReservationError.NoProjectsProvided());
            
            if(data.TotalPrice is not null && data.TotalPrice < 0)
                return Result<Reservation>.Failure(ReservationError.InvalidTotalPrice(data.TotalPrice.Value));
            
            var reservedAtResult = ValidateReservedAt(data.ReservedAt, today);
            if(!reservedAtResult.IsSuccess)
                return Result<Reservation>.Failure(reservedAtResult.Errors[0]);
            
            var reservation = new Reservation
            {
                AdminId = data.AdminId,
                StudentId = data.StudentId,
                ReservationStatus = data.ReservationStatus,
                PaidAmount = 0,
                ReservedAt = data.ReservedAt,
                ReservationType = ReservationType.Project
            };
            
            var addProjectsResult = reservation.AddProjects(projects);
            
            if (!addProjectsResult.IsSuccess)
                return Result<Reservation>.Failure(addProjectsResult.Errors[0]);
            
            reservation.TotalPrice = reservation.CalculateTotalPrice(data.TotalPrice);
            reservation.DiscountRate = reservation.CalculateDiscountRate(reservation.TotalPrice);
            reservation.PaymentStatus = reservation.DeterminePaymentStatus(reservation.PaidAmount, reservation.TotalPrice, today);
            
            return Result<Reservation>.Success(reservation);
        }
        
        public Result UpdateReservationStatus(ReservationStatus newStatus)
        {
            if(IsDeleted)
                return Result.Failure(ReservationError.CantUpdateAnArchivedReservation(Id));
            
            ReservationStatus = newStatus;
            return Result.Success();
        }

        public Result UpdateReservedAt(DateOnly reservedAt, DateOnly today)
        {
            if(IsDeleted)
                return Result.Failure(ReservationError.CantUpdateAnArchivedReservation(Id));
            
            var validationResult = ValidateReservedAt(reservedAt, today);
            if(!validationResult.IsSuccess)
                return validationResult;

            var projectStartingEarlier = Projects.FirstOrDefault(p => p.StartDate < reservedAt);
            if(projectStartingEarlier is not null)
                return Result.Failure(ReservationError.ProjectStartsBeforeReservation(projectStartingEarlier.StartDate, reservedAt));
            
            ReservedAt = reservedAt;
            return Result.Success();
        }

        public Result UpdateTotalPrice(decimal? totalPrice, DateOnly today)
        {
            var total = CalculateTotalPrice(totalPrice);
            
            if (total < 0)
                return Result.Failure(ReservationError.InvalidTotalPrice(total));

            TotalPrice = total;
            RecalculatePaymentDetails(today);

            return Result.Success();
        }

        public Result<PaymentTransaction> RegisterPayment(decimal paidAmount, DateTime transactionDate, DateOnly today, string? note = null)
        {
            if (paidAmount <= 0)
                return Result<PaymentTransaction>.Failure(ReservationError.InvalidAmount(paidAmount));

            var moneyResult = Money.Create(paidAmount);
            if (!moneyResult.IsSuccess)
                return Result<PaymentTransaction>.Failure(moneyResult.Errors[0]);
            
            var newTransactionResult = PaymentTransaction.Create(moneyResult.Payload!, PaymentTransactionType.Payment, transactionDate, Id, note);

            if(!newTransactionResult.IsSuccess) 
                return Result<PaymentTransaction>.Failure(newTransactionResult.Errors[0]);
            
            _paymentTransactions.Add(newTransactionResult.Payload!);
            
            PaidAmount += newTransactionResult.Payload!.Amount.Value;

            PaymentStatus = DeterminePaymentStatus(PaidAmount, TotalPrice, today);
            Bonus = CalculateBonus(TotalPrice, PaidAmount);

            return Result<PaymentTransaction>.Success(newTransactionResult.Payload!);
        }
        
        public Result<PaymentTransaction> SubtractPayment(
            decimal amount,
            DateTime transactionDate,
            PaymentTransactionType paymentType,
            DateOnly today,
            string? note = null)
        {
            if (paymentType == PaymentTransactionType.Payment)
                return Result<PaymentTransaction>.Failure(PaymentTransactionError.InvalidPaymentType(paymentType));

            if (amount <= 0)
                return Result<PaymentTransaction>.Failure(ReservationError.InvalidAmount(amount, "Amount to subtract cannot be negative."));
            
            var moneyResult = Money.Create(amount);
            if (!moneyResult.IsSuccess)
                return Result<PaymentTransaction>.Failure(moneyResult.Errors[0]);
            
            if (amount > PaidAmount)
                return Result<PaymentTransaction>.Failure(ReservationError.InvalidAmount(amount, "Cannot subtract more than the paid amount."));
            
            var newTransactionResult = PaymentTransaction.Create(moneyResult.Payload!, paymentType, transactionDate, Id, note);

            if(!newTransactionResult.IsSuccess) 
                return Result<PaymentTransaction>.Failure(newTransactionResult.Errors[0]);

            _paymentTransactions.Add(newTransactionResult.Payload!);

            PaidAmount -= amount;

            PaymentStatus = DeterminePaymentStatus(PaidAmount, TotalPrice, today);
            Bonus = CalculateBonus(TotalPrice, PaidAmount);

            return Result<PaymentTransaction>.Success(newTransactionResult.Payload!);
        }
        
        public void UpdateNotes(string? notes)
        {
            Notes = notes;
        }
        
        public void ChangeStudent(string studentId)
        {
            StudentId = studentId;
        }

        public Result UpdateClasses(IReadOnlyList<ClassUpdateData> classesToUpdate, DateOnly today)
        {
            if(ReservationType != ReservationType.Class)
                return Result.Failure(ReservationError.NotAClassReservation(Id));

            foreach (var classUpdate in classesToUpdate)
            {
                var existingClass = Classes.FirstOrDefault(c => c.Id == classUpdate.Id);

                if (existingClass is null)
                    return Result.Failure(ClassError.NotFound(classUpdate.Id));

                var updateResult = existingClass.Update(classUpdate.SubjectId, classUpdate.Price, classUpdate.StartsAt, classUpdate.EndsAt);

                if (!updateResult.IsSuccess)
                    return Result.Failure(updateResult.Errors[0]);
            }
            
            RecalculatePaymentDetails(today);
            
            return Result.Success();
        }
        
        public Result UpdateProjects(IReadOnlyList<ProjectUpdateData> projectsToUpdate, DateOnly today)
        {
            if(ReservationType != ReservationType.Project)
                return Result.Failure(ReservationError.NotAProjectReservation(Id));

            foreach (var projectUpdate in projectsToUpdate)
            {
                var existingProject = Projects.FirstOrDefault(p => p.Id == projectUpdate.Id);

                if (existingProject is null)
                    return Result.Failure(ProjectError.NotFound(projectUpdate.Id));

                var updateResult = existingProject.Update(projectUpdate);

                if (!updateResult.IsSuccess)
                    return updateResult;
            }

            RecalculatePaymentDetails(today);

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

                _classes.Add(classResult.Payload!);
            }

            return Result.Success();
        }

        public Result AddProjects(IReadOnlyList<ProjectCreateData> projectsToAdd)
        {
            if(ReservationType != ReservationType.Project)
                return Result.Failure(ReservationError.NotAProjectReservation(Id));

            foreach (var projectData in projectsToAdd)
            {
                if(projectData.StartDate < ReservedAt)
                    return Result.Failure(ReservationError.ProjectStartsBeforeReservation(projectData.StartDate, ReservedAt));
                
                var projectResult = Project.Create(projectData);
                if (!projectResult.IsSuccess)
                    return Result.Failure(projectResult.Errors[0]);

                _projects.Add(projectResult.Payload!);
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
        
        public Result RestoreClassReservation()
        {
            if(ReservationType != ReservationType.Class)
                return Result.Failure(ReservationError.NotAClassReservation(Id));

            var result = Restore();
            if(!result.IsSuccess)
                return result;

            foreach (var @class in Classes.Where(c => c.IsDeleted))
            {
                var restoreClassResult = @class.Restore();

                if(!restoreClassResult.IsSuccess)
                    return restoreClassResult;
            }

            return Result.Success();
        }

        public Result RestoreProjectReservation()
        {
            if(ReservationType != ReservationType.Project)
                return Result.Failure(ReservationError.NotAProjectReservation(Id));

            var result = Restore();
            if(!result.IsSuccess)
                return result;

            foreach (var project in Projects.Where(p => p.IsDeleted))
            {
                var restoreProjectResult = project.Restore();

                if(!restoreProjectResult.IsSuccess)
                    return restoreProjectResult;
            }

            return Result.Success();
        }

        public Result DeleteProjects(IReadOnlyList<int> projectIdsToDelete)
        {
            if (projectIdsToDelete.Count == 0)
                return Result.Success();

            var existingProjectIds = _projects.Select(p => p.Id).ToHashSet();
            var invalidProjectIds = projectIdsToDelete.Where(id => !existingProjectIds.Contains(id)).ToList();

            if (invalidProjectIds.Count > 0)
                return Result.Failure(ProjectError.NotFound(invalidProjectIds[0]));
            
            _projects.RemoveAll(p => projectIdsToDelete.Contains(p.Id));
            
            return Result.Success();
        }

        public bool RequiresHistoryRetention(DateOnly today)
            => ReservationStatus == ReservationStatus.Completed
               || PaymentStatus != PaymentStatus.Pending
               || Classes.Any(c => c.StartsAt <= DateTime.UtcNow)
               || Projects.Any(p => p.StartDate <= today);
        
        public void RemoveClass(Class classToRemove) 
            => _classes.Remove(classToRemove);

        public void RemoveProject(Project projectToRemove)
           => _projects.Remove(projectToRemove);
        
        
        private PaymentStatus DeterminePaymentStatus(decimal paidAmount, decimal totalPrice, DateOnly today)
        {
            var isPastDue = ReservationType == ReservationType.Class
                ? Classes.Count != 0 && Classes.Max(c => c.EndsAt) < DateTime.UtcNow
                : Projects.Count != 0 && Projects.Max(p => p.EndDate) < today;

            return paidAmount switch
            {
                var amount when amount >= totalPrice => PaymentStatus.Paid,
                var amount when amount < totalPrice && isPastDue => PaymentStatus.Overdue,
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

        private void RecalculatePaymentDetails(DateOnly today)
        {
            DiscountRate = CalculateDiscountRate(TotalPrice);
            Bonus = CalculateBonus(TotalPrice, PaidAmount);
            PaymentStatus = DeterminePaymentStatus(PaidAmount, TotalPrice, today);
        }

        private static Result ValidateReservedAt(DateOnly reservedAt, DateOnly today)
        {
            if(reservedAt <= DateOnly.MinValue || reservedAt > today)
                return Result.Failure(ReservationError.InvalidReservedAt(reservedAt));
            
            return Result.Success();
        }

        private Result Delete()
        {
            if(IsDeleted) 
                return Result.Failure(ReservationError.AlreadyDeleted(Id));

            IsDeleted = true;
            DeletedAt = DateTime.UtcNow;
            return Result.Success();
        }

        private Result Restore()
        {
            if(!IsDeleted)
                return Result.Failure(ReservationError.NotArchived(Id));

            IsDeleted = false;
            DeletedAt = null;
            return Result.Success();
        }
    }
}
