namespace CodeWithMixx.API.Domain;

public interface IAuditable
{
    public DateTime CreatedAt { get; }
    public DateTime? UpdatedAt { get; }
}