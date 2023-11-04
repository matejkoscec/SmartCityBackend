namespace SmartCityBackend.Models;

public class ReservationHistory : AuditableEntity
{
    public DateTime Start { get; set; }

    public DateTime End { get; set; }

    public long UserId { get; set; }
    public User User { get; set; } = null!;

    public Guid ParkingSpotId { get; set; }
    public ParkingSpot ParkingSpot { get; set; } = null!;
    
    public IEnumerable<ParkingSpotHistory> ParkingSpotsHistory { get; set; } = null!;
}