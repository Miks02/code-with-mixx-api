using CodeWithMixx.API.Common.Result;

namespace CodeWithMixx.API.Domain.Entities.Projects;

public static class ProjectError
{
    public static Error NotFound(int identifier)
        => new Error("Project.NotFound", $"The project with identifier '{identifier}' was not found.", ErrorType.NotFound);
    
    public static Error NegativePrice() 
        => new Error("Project.NegativePrice", "The price of the project cannot be negative.", ErrorType.Validation);    
    
    public static Error InvalidProgress(decimal progress)
        => new Error("Project.InvalidProgress", $"The project progress '{progress}' is invalid. It must be between 0 and 100.", ErrorType.Validation);

    public static Error InvalidDateRange(DateTime startDate, DateTime endDate)
        => new Error("Project.InvalidDateRange", $"The project start date '{startDate}' cannot be after the end date '{endDate}'.", ErrorType.Validation);
    
    public static Error GithubLinkIsTooLong(string githubLink)
        => new Error("Project.GithubLinkIsTooLong", $"The GitHub link '{githubLink}' is too long. It must be less than 200 characters.", ErrorType.Validation);
    public static Error DownloadLinkIsTooLong(string downloadLink)
        => new Error("Project.DownloadLinkIsTooLong", $"The download link '{downloadLink}' is too long. It must be less than 200 characters.", ErrorType.Validation);
    
    public static Error EmptyProjectNote()
        => new Error("Project.EmptyProjectNote", "The project note cannot be empty.", ErrorType.Validation);
    
    public static Error InvalidReservationDate(DateTime reservedAt)
        => new Error("Project.InvalidReservationDate", $"The reservation date '{reservedAt}' is invalid. It must be between the minimum value and the current date.", ErrorType.Validation);
    
    public static Error AlreadyDeleted(int identifier)
        => new Error("Project.AlreadyDeleted", $"The project with identifier '{identifier}' has already been deleted.", ErrorType.Conflict);

    public static Error NotArchived(int identifier)
        => new Error("Project.NotArchived", $"The project with identifier '{identifier}' is not archived.", ErrorType.Conflict);

    public static Error ReservationArchived(int identifier, int reservationIdentifier)
        => new Error("Project.ReservationArchived", $"The project with identifier '{identifier}' cannot be restored because its reservation '{reservationIdentifier}' is archived. Restore the reservation instead.", ErrorType.Conflict);
}