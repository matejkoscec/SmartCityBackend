namespace SmartCityBackend.Models;

public class User : AuditableEntity
{
    public string Name { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string PreferredUsername { get; set; } = null!;

    public string GivenName { get; set; } = null!;

    public string FamilyName { get; set; } = null!;

    public bool EmailVerified { get; set; }

    public IEnumerable<Role> Roles { get; set; } = new List<Role>();

    public IEnumerable<ActiveReservation> ActiveReservations { get; set; } = new List<ActiveReservation>();

    public IEnumerable<ReservationHistory> ReservationsHistory { get; set; } = new List<ReservationHistory>();
    
    public IEnumerable<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}