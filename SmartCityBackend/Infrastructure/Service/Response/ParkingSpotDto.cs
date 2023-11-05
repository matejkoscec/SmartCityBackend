using System.Text.Json.Serialization;
using SmartCityBackend.Models;

namespace SmartCityBackend.Infrastructure.Service.Response;

public record ParkingSpotDto(string Id,
    decimal Latitude,
    decimal Longitude,
    ParkingZone ParkingSpotZone,
    bool Occupied,
    string OccupiedTimestamp);