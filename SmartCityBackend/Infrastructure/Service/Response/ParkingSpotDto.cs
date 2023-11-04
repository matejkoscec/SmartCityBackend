namespace SmartCityBackend.Infrastructure.Service.Response;

public record ParkingSpotDto(string? Id,
    double Latitude,
    double Longitude,
    ParkingSpotZone ParkingSpotZone,
    bool Occupied,
    string OccupiedTimestamp);

public enum ParkingSpotZone
{
    Zone1,
    Zone2,
    Zone3,
    Zone4
}