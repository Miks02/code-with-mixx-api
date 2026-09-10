namespace CodeWithMixx.API.Features.Classes.Common;

public record ClassItem
{
    public int Id { get; init; }
    public int SubjectId { get; init; }
    public string SubjectName { get; init; } = null!;
    public decimal Price { get; init; }
    public DateTime StartsAt { get; init; }
    public DateTime EndsAt { get; init; }
};