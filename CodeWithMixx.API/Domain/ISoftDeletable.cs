namespace CodeWithMixx.API.Domain;

public interface ISoftDeletable 
{
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
}