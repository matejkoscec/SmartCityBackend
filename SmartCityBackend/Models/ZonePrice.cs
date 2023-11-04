namespace SmartCityBackend.Models;

public class ZonePrice : AuditableEntity
{
    public ParkingZone Zone { get; set; }
    
    public decimal Price { get; set; }
    
    public IEnumerable<ParkingSpotHistory> ParkingSpotsHistory { get; set; } = null!;
}