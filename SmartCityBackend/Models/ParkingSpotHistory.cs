namespace SmartCityBackend.Models;

public class ParkingSpotHistory : AuditableEntity
{
    public bool IsOccupied { get; set; }

    public DateTimeOffset StartTime { get; set; }

    public Guid ParkingSpotId { get; set; }
    public ParkingSpot ParkingSpot { get; set; } = null!;

    public long? ActiveReservationId { get; set; }
    public ActiveReservation ActiveReservation { get; set; } = null!;

    public long? ReservationHistoryId { get; set; }
    public ReservationHistory? ReservationHistory { get; set; } = null!;

    public long ZonePriceId { get; set; }
    public ZonePrice ZonePrice { get; set; } = null!;
}