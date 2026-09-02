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

    }
}
