namespace SmartCityBackend.Models;

public class ParkingSpot : AuditableEntity<Guid>
{
    public decimal Lat { get; set; }
    
    public decimal Lng { get; set; }

    public ParkingZone Zone { get; set; }
    
    public IEnumerable<ParkingSpotHistory> ParkingSpotsHistory { get; set; } = null!;
    
    public IEnumerable<ActiveReservation> ActiveReservations { get; set; } = null!;
}