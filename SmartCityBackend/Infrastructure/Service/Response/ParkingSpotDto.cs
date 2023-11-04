using SmartCityBackend.Models;

namespace SmartCityBackend.Infrastructure.Service.Response;

public record ParkingSpotDto(string Id,
    decimal Latitude,
    decimal Longitude,
    ParkingZone ParkingZone,
    bool Occupied,
    string OccupiedTimestamp);