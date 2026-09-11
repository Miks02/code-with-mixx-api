using CodeWithMixx.API.Common.Results;
using CodeWithMixx.API.Domain.Entities.Admins;
using CodeWithMixx.API.Domain.Entities.Classes;
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

        public ICollection<Class> Classes { get; private set; } = [];
        
        private Reservation() {}

        public static Result<Reservation> CreateClassReservation(ReservationCreateData data)
        {
            if (data.Classes.Count <= 0)
                return Result<Reservation>.Failure(ReservationError.NotFound());
            
            if(data.TotalPrice is not null && data.TotalPrice < 0)
                return Result<Reservation>.Failure(ReservationError.InvalidTotalPrice(data.TotalPrice.Value));
            
            if(data.PaidAmount < 0)
                return Result<Reservation>.Failure(ReservationError.InvalidAmount(data.PaidAmount));
            
            var totalPrice = data.TotalPrice is null
                ? data.Classes.Sum(c => c.Price) 
                : data.TotalPrice.Value;
            
            var reservation = new Reservation
            {
                AdminId = data.AdminId,
                StudentId = data.StudentId,
                ReservationStatus = data.ReservationStatus,
                TotalPrice = totalPrice,
                PaidAmount = data.PaidAmount,
                CreatedAt = DateTime.UtcNow,
            };
            
            reservation.DiscountRate = reservation.CalculateDiscountRate(totalPrice);
            reservation.Bonus = reservation.CalculateBonus(totalPrice, data.PaidAmount);
            reservation.PaymentStatus = reservation.DeterminePaymentStatus(data.PaidAmount, totalPrice);
            
            foreach (var c in data.Classes)
            {
                var classResult = Class.Create(c.SubjectId, c.Price, c.StartsAt, c.EndsAt);

                if (!classResult.IsSuccess)
                    return Result<Reservation>.Failure(classResult.Errors[0]);

                reservation.Classes.Add(classResult.Payload!);
            }
            
            return Result<Reservation>.Success(reservation);
        }
        
        public Result UpdateReservationStatus(ReservationStatus newStatus)
        {
            ReservationStatus = newStatus;
            UpdatedAt = DateTime.UtcNow;

            return Result.Success();
        }

        public Result UpdateTotalPrice(decimal totalPrice)
        {
            if (totalPrice < 0)
                return Result.Failure(ReservationError.InvalidTotalPrice(totalPrice));

            TotalPrice = totalPrice;
            DiscountRate = CalculateDiscountRate(totalPrice);
            Bonus = CalculateBonus(totalPrice, PaidAmount);
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
        
        public Result UpdateNotes(string? notes)
        {
            Notes = notes;
            UpdatedAt = DateTime.UtcNow;

            return Result.Success();
        }
        
        public void ChangeStudent(string studentId)
        {
            StudentId = studentId;
            UpdatedAt = DateTime.UtcNow;
        }
        
        private PaymentStatus DeterminePaymentStatus(decimal paidAmount, decimal totalPrice)
        {
            return paidAmount switch
            {
                var amount when amount >= totalPrice => PaymentStatus.Paid,
                var amount when amount < totalPrice && Classes.Select(c => c.EndsAt).Last() < DateTime.UtcNow => PaymentStatus.Overdue,
                var amount when amount > 0 && amount < totalPrice => PaymentStatus.PartiallyPaid,
                _ => PaymentStatus.Pending
            };
        }
        
        private decimal SumDefaultPriceOfClasses()
        {
            return Classes.Sum(c => c.Price);
        }
        
        private decimal CalculateDiscountRate(decimal requestedTotalPrice)
        {
            var defaultTotal = SumDefaultPriceOfClasses();
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

        public bool RequiresHistoryRetention()
            => ReservationStatus == ReservationStatus.Completed
               || PaymentStatus != PaymentStatus.Pending
               || Classes.Any(c => c.StartsAt <= DateTime.UtcNow);

        public void Delete()
        {
            IsDeleted = true;
            DeletedAt = DateTime.UtcNow;
        }
    }
}
