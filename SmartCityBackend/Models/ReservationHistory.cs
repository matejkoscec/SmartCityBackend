﻿namespace SmartCityBackend.Models;

public class ReservationHistory : AuditableEntity
{
    public DateTimeOffset Start { get; set; }

    public DateTimeOffset End { get; set; }

    public long UserId { get; set; }
    public User User { get; set; } = null!;

    public Guid ParkingSpotId { get; set; }
    public ParkingSpot ParkingSpot { get; set; } = null!;
    
    public IEnumerable<ParkingSpotHistory> ParkingSpotsHistory { get; set; } = null!;
}