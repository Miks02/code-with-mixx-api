namespace CodeWithMixx.API.Domain;

public interface ISoftDeletable 
{
    public bool IsDeleted { get; }
    public DateTime? DeletedAt { get; }
}