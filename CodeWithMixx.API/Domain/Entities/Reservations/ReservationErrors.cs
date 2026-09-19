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

        public static Error InvalidAmount(decimal amount, string message = "Amount must be positive")
            => new("Reservation.InvalidAmount", $"Amount '{amount}' is invalid: {message}", ErrorType.Validation);
        
        public static Error InvalidTotalPrice(decimal totalPrice)
            => new("Reservation.InvalidTotalPrice", $"Total price '{totalPrice}' must be positive", ErrorType.Validation);
        
        public static Error NoClassesProvided()
            => new("Reservation.NoClassesProvided", "No classes have been provided for a class reservation", ErrorType.Validation);
        
        public static Error NoProjectsProvided()
            => new("Reservation.NoProjectsProvided", "No projects have been provided for a project reservation", ErrorType.Validation);
        
        public static Error NotAClassReservation(int? identifier = null)
        {
            string message = identifier is null
                ? "Reservation is not a class reservation"
                : $"Reservation with identifier '{identifier}' is not a class reservation";

            return new Error("Reservation.NotAClassReservation", message, ErrorType.Validation);
        }
        
        public static Error NotAProjectReservation(int? identifier = null)
        {
            string message = identifier is null
                ? "Reservation is not a project reservation"
                : $"Reservation with identifier '{identifier}' is not a project reservation";

            return new Error("Reservation.NotAProjectReservation", message, ErrorType.Validation);
        }
        
        public static Error AlreadyDeleted(int identifier)
        {
            string message = $"Reservation with identifier '{identifier}' is already deleted";
            return new Error("Reservation.AlreadyDeleted", message, ErrorType.Conflict);
        }
    }
}
