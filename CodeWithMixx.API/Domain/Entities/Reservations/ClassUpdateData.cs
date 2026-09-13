namespace CodeWithMixx.API.Domain.Entities.Reservations;

public class ClassUpdateData
{
    public int Id { get; set; }
    public int SubjectId { get; set; }
    public decimal Price { get; set; }
    public DateTime StartsAt { get; set; }
    public DateTime EndsAt { get; set; }
}