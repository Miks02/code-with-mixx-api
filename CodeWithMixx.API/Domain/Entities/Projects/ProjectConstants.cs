namespace CodeWithMixx.API.Domain.Entities.Projects;

public static class ProjectConstants
{
    public static readonly DateOnly MinStartDate = new(2000, 1, 1);
    public const int MaxNoteLength = 500;
    public const int MaxLinkLength = 200;
}
