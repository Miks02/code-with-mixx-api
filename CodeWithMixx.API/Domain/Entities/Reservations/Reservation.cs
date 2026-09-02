using CodeWithMixx.API.Domain.Entities.Admins;
using CodeWithMixx.API.Domain.Entities.Classes;
using CodeWithMixx.API.Domain.Entities.Students;

namespace CodeWithMixx.API.Domain.Entities.Reservations
{
    public class Reservation : IAuditable
    {
        public int Id { get; set; }
        public ReservationStatus ReservationStatus { get; set; } = ReservationStatus.Confirmed;
        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;

        public decimal TotalPrice { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal DiscountRate { get; set; }
        public decimal Bonus { get; set; }

        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public Admin Admin { get; set; } = null!;
        public string AdminId { get; set; } = null!;

        public Student Student { get; set; } = null!;
        public string StudentId { get; set; } = null!;

        public ICollection<Class> Classes { get; set; } = [];


    }
}
