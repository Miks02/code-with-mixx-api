using CodeWithMixx.API.Common.Result;

namespace CodeWithMixx.API.Domain.Entities.Reservations
{
    public static class ReservationError
    {
        public static Error NotFound(int? identifier = null)
        {
            string message = identifier is null
                ? "Reservation not found"
                : $"Reservation with identifier '{identifier}' is not found";

            return new Error("Reservation.NotFound", message, ErrorType.NotFound);
        }

        public static Error AlreadyPaid(int? identifier = null)
        {
            string message = identifier is null
                ? "Reservation is already fully paid"
                : $"Reservation with identifier '{identifier}' is already fully paid";

            return new Error("Reservation.AlreadyPaid", message, ErrorType.Validation);
        }

        public static Error InvalidAmount(decimal amount)
            => new("Reservation.InvalidAmount", $"Amount '{amount}' must be positive", ErrorType.Validation);
        
        public static Error InvalidTotalPrice(decimal totalPrice)
            => new("Reservation.InvalidTotalPrice", $"Total price '{totalPrice}' must be positive", ErrorType.Validation);
    }
}
