using CodeWithMixx.API.Common.Results;
using CodeWithMixx.API.Domain.Entities.Admins;
using CodeWithMixx.API.Domain.Entities.Classes;
using CodeWithMixx.API.Domain.Entities.Students;

namespace CodeWithMixx.API.Domain.Entities.Reservations
{
    public class Reservation : IAuditable
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

            var paymentStatus = data switch
            {
                var d when d.PaidAmount >= d.TotalPrice => PaymentStatus.Paid,
                var d when d.PaidAmount < d.TotalPrice && d.Classes.Select(c => c.EndsAt).Last() < DateTime.UtcNow => PaymentStatus.Overdue,
                var d when d.PaidAmount > 0 && d.PaidAmount < d.TotalPrice => PaymentStatus.PartiallyPaid,
                _ => PaymentStatus.Pending
            };
            
            var totalPrice = data.TotalPrice is null
                ? data.Classes.Sum(c => c.Price) 
                : data.TotalPrice.Value;
            
            var reservation = new Reservation
            {
                AdminId = data.AdminId,
                StudentId = data.StudentId,
                ReservationStatus = data.ReservationStatus,
                PaymentStatus = paymentStatus,
                TotalPrice = totalPrice,
                PaidAmount = data.PaidAmount,
                CreatedAt = DateTime.UtcNow
            };
            
            foreach (var c in data.Classes)
            {
                var classResult = Class.Create(c.SubjectId, c.Price, c.StartsAt, c.EndsAt);

                if (!classResult.IsSuccess)
                    return Result<Reservation>.Failure(classResult.Errors[0]);

                reservation.Classes.Add(classResult.Payload!);
            }
            
            return Result<Reservation>.Success(reservation);
        }


    }
}
