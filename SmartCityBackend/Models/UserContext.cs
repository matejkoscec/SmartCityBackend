namespace SmartCityBackend.Models;

public class UserContext
{
    public long Id { get; set; }
    public string Email { get; set; } = null!;
    public Role Role { get; set; }
    public string PreferredUsername { get; set; } = null!;
    public string GivenName { get; set; } = null!;
    public string FamilyName { get; set; } = null!;
}