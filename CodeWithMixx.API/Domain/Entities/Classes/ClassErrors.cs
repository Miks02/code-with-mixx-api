using CodeWithMixx.API.Common.Result;

namespace CodeWithMixx.API.Domain.Entities.Classes
{
    public static class ClassError
    {
        public static Error NotFound(int? identifier = null)
        {
            string message = identifier is null
                ? "Class not found"
                : $"Class with identifier '{identifier}' is not found";

            return new Error("Class.NotFound", message, ErrorType.NotFound);
        }

        public static Error InvalidSchedule(DateTime startsAt, DateTime endsAt)
            => new("Class.InvalidSchedule", $"Class schedule is invalid: starts at '{startsAt:u}' and ends at '{endsAt:u}'", ErrorType.Validation);
        
        public static Error InvalidPrice(decimal price)
            => new("Class.InvalidPrice", $"Price '{price}' must be positive", ErrorType.Validation);
        
        public static Error AlreadyDeleted(int identifier)
        {
            string message = $"Class with identifier '{identifier}' is already deleted";
            return new Error("Class.AlreadyDeleted", message, ErrorType.Conflict);
        }

        public static Error NotArchived(int identifier)
        {
            string message = $"Class with identifier '{identifier}' is not archived";
            return new Error("Class.NotArchived", message, ErrorType.Conflict);
        }

        public static Error ReservationArchived(int identifier, int reservationIdentifier)
        {
            string message = $"Class with identifier '{identifier}' cannot be restored because its reservation '{reservationIdentifier}' is archived. Restore the reservation instead";
            return new Error("Class.ReservationArchived", message, ErrorType.Conflict);
        }

        public static Error InvalidDuration(int? identifier = null)
        {  
           return identifier is not null
               ? new Error($"Class.InvalidDuration", $"Class '{identifier}' duration must be less than {ClassConstants.MaxDurationInMinutes} minutes", ErrorType.Validation) 
               : new Error("Class.InvalidDuration", $"Class duration must be less than {ClassConstants.MaxDurationInMinutes} minutes", ErrorType.Validation);
        }

        public static Error InvalidReservationDate(int? identifier = null)
        {
            return identifier is not null
                ? new Error($"Class.InvalidReservationDate", $"Class '{identifier}' reservation date must be before the class starts", ErrorType.Validation)
                : new Error("Class.InvalidReservationDate", "Class reservation date must be before the class starts", ErrorType.Validation);
        }
    }
}
