namespace SmartCityBackend.Models;

public class RefreshToken : AuditableEntity
{
    public string Token { get; set; } = null!;
    
    public DateTimeOffset Expires { get; set; }
    
    public long UserId { get; set; }
    
    public User User { get; set; } = null!;
}