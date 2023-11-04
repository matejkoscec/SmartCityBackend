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

    public IEnumerable<Role> Roles { get; set; } = Enumerable.Empty<Role>();

    public IEnumerable<ActiveReservation> ActiveReservations { get; set; } = Enumerable.Empty<ActiveReservation>();

    public IEnumerable<ReservationHistory> ReservationsHistory { get; set; } = Enumerable.Empty<ReservationHistory>();
}