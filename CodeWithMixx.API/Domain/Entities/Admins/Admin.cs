using CodeWithMixx.API.Domain.Entities.Reservations;
using CodeWithMixx.API.Domain.Entities.Users;

namespace CodeWithMixx.API.Domain.Entities.Admins;

public class Admin
{
    public User User { get; private set; } = null!;
    public string UserId { get; private set; } = null!;

    private readonly List<Reservation> _reservations = [];
    public IReadOnlyCollection<Reservation> Reservations => _reservations.AsReadOnly();

    private Admin() {}

    public static Admin Create(string userId)
    {
        return new Admin
        {
            UserId = userId
        };
    }
}